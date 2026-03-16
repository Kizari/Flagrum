// ReSharper disable CppDFAMemoryLeak (Qt widgets handle disposing their children)
#pragma once

#include <QApplication>
#include <QFile>
#include <QLabel>
#include <QMouseEvent>
#include <QPainter>
#include <QPushButton>
#include <QStyle>
#include <QSvgWidget>
#include <QVBoxLayout>
#include <QWindow>

#include "CustomShellWindow.hpp"

/**
 * Qt window that implements Flagrum's custom window shell.
 */
class CustomShellWindow final : public QWidget
{
private:
    QWidget* titleBar_;
    QVBoxLayout* layout_;
    QWidget* container_;

public:
    explicit CustomShellWindow(QWidget* parent = nullptr) : QWidget(parent)
    {
        // Set the window properties
        setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
        setStyleSheet("background: #181512;");
        move(QApplication::primaryScreen()->geometry().center() - rect().center());

        // Create the content layout
        layout_ = new QVBoxLayout(this);
        layout_->setContentsMargins(2, 2, 2, 2);
        layout_->setSpacing(0);

        // Create the title bar
        titleBar_ = new QWidget(this);
        titleBar_->setFixedHeight(42);
        titleBar_->setStyleSheet("background: #181512;");
        titleBar_->setContentsMargins(10, 0, 0, 0);

        // Load logo file
        auto file = QFile(":/Resources/logo.svg");
        if (!file.open(QIODevice::ReadOnly))
        {
            throw std::runtime_error("Failed to open logo.svg");
        }
        
        auto svgData = static_cast<QString>(file.readAll());
        file.close();

        // Create the application icon
        const auto wrapper = new QWidget(titleBar_); // Needed to align the logo to center
        const auto wrapperLayout = new QHBoxLayout(wrapper);
        wrapperLayout->setContentsMargins(0, 0, 0, 3);
        const auto logo = new QSvgWidget(wrapper);
        svgData.replace("#ffffff", "#837363", Qt::CaseInsensitive);
        logo->load(svgData.toUtf8());
        logo->setFixedSize(20, 20);
        wrapperLayout->addWidget(logo);

        // Create the application title
        const auto title = new QLabel("Flagrum", titleBar_);
        title->setStyleSheet("color: #837363; font-size: 14px;");
        title->setAlignment(Qt::AlignLeft | Qt::AlignVCenter);
        title->setContentsMargins(2, 5, 15, 8);

        // Create the window buttons
        const auto minimize = CreateTitleBarButton(
            titleBar_,
            "window-minimize",
            QStyle::SP_TitleBarMinButton);

        // TODO: Icon should change when maximized state changes
        const auto maximize = CreateTitleBarButton(
            titleBar_,
            "window-maximize",
            QStyle::SP_TitleBarMaxButton);

        const auto closeButton = CreateTitleBarButton(
            titleBar_,
            "window-close",
            QStyle::SP_TitleBarCloseButton);

        // Populate the title bar layout
        const auto titleBarLayout = new QHBoxLayout(titleBar_);
        titleBarLayout->setContentsMargins(0, 0, 0, 0);
        titleBarLayout->addWidget(wrapper);
        titleBarLayout->addWidget(title);
        titleBarLayout->addStretch();
        titleBarLayout->addWidget(minimize);
        titleBarLayout->addWidget(maximize);
        titleBarLayout->addWidget(closeButton);
        titleBar_->setLayout(titleBarLayout);
        layout_->addWidget(titleBar_);

        // Connect window buttons
        connect(minimize, &QPushButton::clicked, this, &QWidget::showMinimized);
        connect(maximize, &QPushButton::clicked, this, [&]
        {
            isMaximized() ? showNormal() : showMaximized();
        });
        connect(closeButton, &QPushButton::clicked, this, &QWidget::close);

        // Create the content container
        container_ = new QWidget(this);
        layout_->addWidget(container_);

        // Enable mouse tracking to handle cursor changes for window edges
        setMouseTracking(true);
        titleBar_->setMouseTracking(true);
        container_->setMouseTracking(true);

        // Enable event handling
        installEventFilter(this);
        titleBar_->installEventFilter(this);
        container_->installEventFilter(this);
    }

    /**
     * Sets the main content widget for this window.
     * 
     * @param widget Widget to set as the window content.
     */
    void SetContentWidget(QWidget* widget) const
    {
        // Clean up existing content widget if one is already present
        if (container_->layout())
        {
            delete container_->layout();
        }

        // Set the content widget
        // ReSharper disable once CppDFAMemoryLeak (Qt parenting handles cleanup)
        const auto layout = new QVBoxLayout(container_);
        layout->setContentsMargins(0, 0, 0, 0);
        layout->addWidget(widget);
    }

protected:
    bool eventFilter(QObject* object, QEvent* event) override
    {
        // Remove/restore window borders on maximize/restore
        if (object == this && event->type() == QEvent::WindowStateChange)
        {
            if (isMaximized())
            {
                layout_->setContentsMargins(0, 0, 0, 0);
            }
            else
            {
                layout_->setContentsMargins(2, 2, 2, 2);
            }
        }

        // Handle maximize/restore via double-click on title bar
        if (object == titleBar_ && event->type() == QEvent::MouseButtonDblClick)
        {
            const auto mouseEvent = static_cast<QMouseEvent*>(event);
            if (mouseEvent->button() == Qt::LeftButton)
            {
                if (isMaximized())
                {
                    showNormal();
                }
                else
                {
                    showMaximized();
                }

                return true;
            }
        }

        // Handle dragging window around by title bar
        // TODO: Prevent cursor change here before mouse actually drags
        if (object == titleBar_ && event->type() == QEvent::MouseButtonPress)
        {
            const auto mouseEvent = static_cast<QMouseEvent*>(event);
            if (mouseEvent->button() == Qt::LeftButton && windowHandle())
            {
                windowHandle()->startSystemMove();
                return true;
            }
        }

        // Skip resize logic if window is maximized
        if (isMaximized())
        {
            return QWidget::eventFilter(object, event);
        }

        // Handle window resizing
        if (event->type() == QEvent::MouseButtonPress)
        {
            const auto mouseEvent = static_cast<QMouseEvent*>(event);
            if (mouseEvent->button() == Qt::LeftButton && windowHandle())
            {
                const auto edges = HitTestEdges(mouseEvent->position());
                if (edges != Qt::Edges{})
                {
                    windowHandle()->startSystemResize(edges);
                    return true;
                }
            }
        }

        // Handle resize cursor change
        if (event->type() == QEvent::MouseMove)
        {
            const auto mouseEvent = static_cast<QMouseEvent*>(event);
            const auto edges = HitTestEdges(mouseEvent->position());

            if (edges == (Qt::TopEdge | Qt::LeftEdge) || edges == (Qt::BottomEdge | Qt::RightEdge))
                setCursor(Qt::SizeFDiagCursor);
            else if (edges == (Qt::TopEdge | Qt::RightEdge) || edges == (Qt::BottomEdge | Qt::LeftEdge))
                setCursor(Qt::SizeBDiagCursor);
            else if (edges.testFlag(Qt::LeftEdge) || edges.testFlag(Qt::RightEdge))
                setCursor(Qt::SizeHorCursor);
            else if (edges.testFlag(Qt::TopEdge) || edges.testFlag(Qt::BottomEdge))
                setCursor(Qt::SizeVerCursor);
            else
                unsetCursor();
        }

        return QWidget::eventFilter(object, event);
    }

private:
    /**
     * Creates a window button suitable for the title bar.
     * 
     * @param parent Widget that will own the button.
     * @param themeName Identifier for the button icon in the OS theme.
     * @param fallback Standard Qt icon to fall back on if the OS icon is not available.
     * @return Pointer to the new button.
     */
    static QPushButton* CreateTitleBarButton(
        QWidget* parent,
        const QString& themeName,
        const QStyle::StandardPixmap fallback)
    {
        const auto button = new QPushButton(parent);
        button->setFixedSize(40, 42);
        button->setFlat(true);
        button->setFocusPolicy(Qt::NoFocus);
        button->setContentsMargins(0, 0, 0, 0);

        auto icon = QIcon::fromTheme(themeName);
        if (icon.isNull())
        {
            icon = button->style()->standardIcon(fallback);
        }

        button->setIcon(icon);
        button->setIconSize(QSize(16, 16));
        button->setStyleSheet(R"(
            QPushButton {
                background: transparent;
                border: none;
                border-radius: 0;
                padding: 0;
                margin: 0;
            }
            QPushButton:hover {
                background: rgba(255,255,255,0.15);
            }
            QPushButton:pressed {
                background: rgba(255,255,255,0.25);
            }
            QPushButton:focus {
                outline: none;
                border: none;
            }
        )");

        return button;
    }
    
    /**
     * Determines which window edges the cursor is hovering, if any.
     * 
     * @param position Position of the cursor.
     */
    Qt::Edges HitTestEdges(const QPointF& position) const
    {
        constexpr int thickness = 2;
        Qt::Edges edges;

        if (position.x() <= thickness) edges |= Qt::LeftEdge;
        if (position.x() >= width() - thickness) edges |= Qt::RightEdge;
        if (position.y() <= thickness) edges |= Qt::TopEdge;
        if (position.y() >= height() - thickness) edges |= Qt::BottomEdge;

        return edges;
    }
};
