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
#include <QWindow>

#include "LocalizationService.hpp"
#include "MainWebView.hpp"
#include "MainWindow.hpp"

/**
 * Main window for the application.
 */
class MainWindow final : public QWidget
{
private:
    QWidget* titleBar_;
    QVBoxLayout* layout_;
    MainWebView* webView_;
    QPushButton* patreonButton_;
    void(*patreonButtonCallback_)() = nullptr;

public:
    /**
     * Creates the main application window with a custom shell, and initializes the embedded web view.
     */
    explicit MainWindow(QWidget* parent = nullptr) : QWidget(parent)
    {
        // Set the window properties
        setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
        setWindowTitle(LSTRING("WindowTitle").c_str());
        resize(1680, 1024);
        setStyleSheet("background: #181512;");

        // Create components
        patreonButton_ = CreatePatreonButton();
        titleBar_ = CreateTitleBar();
        webView_ = new MainWebView(this);

        // Create the content layout
        layout_ = new QVBoxLayout(this);
        layout_->setContentsMargins(2, 2, 2, 2);
        layout_->setSpacing(0);
        layout_->addWidget(titleBar_);
        layout_->addWidget(webView_);

        // Enable mouse tracking to handle cursor changes for window edges
        setMouseTracking(true);
        titleBar_->setMouseTracking(true);

        // Enable mouse event handling
        installEventFilter(this);
        titleBar_->installEventFilter(this);
    }

    /**
     * Gets a pointer to the embedded web view.
     */
    [[nodiscard]] MainWebView* GetWebView() const
    {
        return webView_;
    }

    /**
     * Sets the visibility of the Patreon button in the title bar.
     * 
     * @param isVisible Whether the button should be visible.
     */
    void SetPatreonButtonVisible(const bool isVisible) const
    {
        patreonButton_->setVisible(isVisible);
    }

    /**
     * Sets the action to execute when the Patreon button is clicked.
     * 
     * @param callback Action to execute.
     */
    void SetPatreonButtonCallback(void(*callback)())
    {
        patreonButtonCallback_ = callback;
    }

protected:
    /**
     * Handles mouse events for dragging, resizing, and cursor changes of the window shell. 
     * 
     * @param object Sender of the event.
     * @param event Event to process.
     * @return True if the event was handled by this filter, otherwise false.
     */
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
            const auto mouseEvent = dynamic_cast<QMouseEvent*>(event);
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
            const auto mouseEvent = dynamic_cast<QMouseEvent*>(event);
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
            const auto mouseEvent = dynamic_cast<QMouseEvent*>(event);
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
            const auto mouseEvent = dynamic_cast<QMouseEvent*>(event);
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
    QWidget* CreateTitleBar()
    {
        // Create title bar
        const auto bar = new QWidget(this);
        bar->setFixedHeight(42);
        bar->setStyleSheet("background: #181512;");
        bar->setContentsMargins(10, 0, 0, 0);

        // Create the window buttons
        const auto minimize = CreateTitleBarButton(bar,
            "window-minimize",
            QStyle::SP_TitleBarMinButton);

        // TODO: Icon should change when maximized state changes
        const auto maximize = CreateTitleBarButton(bar,
            "window-maximize",
            QStyle::SP_TitleBarMaxButton);

        const auto closeButton = CreateTitleBarButton(bar,
            "window-close",
            QStyle::SP_TitleBarCloseButton);

        // Connect window buttons
        connect(minimize, &QPushButton::clicked, this, &QWidget::showMinimized);
        connect(maximize, &QPushButton::clicked, this, [&]
        {
            isMaximized() ? showNormal() : showMaximized();
        });
        connect(closeButton, &QPushButton::clicked, this, &QWidget::close);

        // Populate the title bar layout
        const auto titleBarLayout = new QHBoxLayout(bar);
        titleBarLayout->setContentsMargins(0, 0, 0, 0);
        titleBarLayout->addWidget(CreateLogo(bar));
        titleBarLayout->addWidget(CreateTitle(bar));
        titleBarLayout->addStretch();
        titleBarLayout->addWidget(patreonButton_);
        titleBarLayout->addSpacerItem(new QSpacerItem(15, 1, QSizePolicy::Fixed, QSizePolicy::Fixed));
        titleBarLayout->addWidget(minimize);
        titleBarLayout->addWidget(maximize);
        titleBarLayout->addWidget(closeButton);
        bar->setLayout(titleBarLayout);
        
        return bar;
    }

    static QWidget* CreateLogo(QWidget* parent)
    {
        // Load logo file
        auto file = QFile(":/Resources/logo.svg");
        if (!file.open(QIODevice::ReadOnly))
        {
            throw std::runtime_error("Failed to open logo.svg");
        }
        
        auto svgData = static_cast<QString>(file.readAll());
        file.close();

        // Create the logo widget
        const auto wrapper = new QWidget(parent); // Needed to align the logo to center
        const auto wrapperLayout = new QHBoxLayout(wrapper);
        wrapperLayout->setContentsMargins(0, 0, 0, 3);
        const auto logo = new QSvgWidget(wrapper);
        svgData.replace("#ffffff", "#837363", Qt::CaseInsensitive);
        logo->load(svgData.toUtf8());
        logo->setFixedSize(20, 20);
        wrapperLayout->addWidget(logo);
        return wrapper;
    }

    static QLabel* CreateTitle(QWidget* parent)
    {
        const auto title = new QLabel(LSTRING("WindowTitle").c_str(), parent);
        title->setStyleSheet("color: #837363; font-size: 14px;");
        title->setAlignment(Qt::AlignLeft | Qt::AlignVCenter);
        title->setContentsMargins(2, 5, 15, 8);
        return title;
    }

    QPushButton* CreatePatreonButton()
    {
        const auto button = new QPushButton(this);
        button->setIcon(QIcon(":/Resources/patreon.png"));
        button->setIconSize(QSize(64, 16));
        button->setFlat(true);
        button->setContentsMargins(0, 0, 0, 5);
        button->setVisible(false);
        button->setStyleSheet("border: none;");
        button->setCursor(Qt::PointingHandCursor);

        connect(button, &QPushButton::clicked, this, [&]
        {
            if (patreonButtonCallback_)
            {
                patreonButtonCallback_();
            }
        });

        return button;
    }
    
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
    [[nodiscard]] Qt::Edges HitTestEdges(const QPointF& position) const
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
