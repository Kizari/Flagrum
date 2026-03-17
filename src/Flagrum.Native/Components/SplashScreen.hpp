// ReSharper disable CppDFAMemoryLeak (Qt widgets handle disposing their children)
#pragma once

#include <QApplication>
#include <QFile>
#include <QHBoxLayout>
#include <QFontDatabase>
#include <QLabel>
#include <QProgressBar>
#include <QScreen>
#include <QSvgWidget>
#include <QWidget>

/**
 * Simple splash screen window for the application.
 */
class SplashScreen final : public QWidget
{
private:
    QLabel* loadingLabel_;
    
public:
    /**
     * Creates the window, sets its properties, and populates its UI.
     * 
     * @param parent Window to own the splash screen.
     */
    explicit SplashScreen(QWidget *parent = nullptr) : QWidget(parent)
    {
        // Set up window
        setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
        setWindowTitle("Flagrum");
        setFixedSize(380, 450);
        setStyleSheet("background: #181512;");
        move(QApplication::primaryScreen()->geometry().center() - rect().center());

        // Create layout
        const auto layout = new QVBoxLayout(this);
        layout->setContentsMargins(20, 30, 20, 20);
        layout->setSpacing(20);

        // Populate layout
        loadingLabel_ = CreateLoadingLabel();
        layout->addWidget(CreateLogo());
        layout->addStretch();
        layout->addWidget(loadingLabel_);
        layout->addWidget(CreateProgressBar());
    }

    /**
     * Sets the text that displays above the splash screen's loading bar.
     * 
     * @param text Text to display.
     */
    void SetLoadingText(const QString& text) const
    {
        loadingLabel_->setText(text);
    }

private:
    QWidget* CreateLogo()
    {
        // Load logo file as text
        auto file = QFile(":/Resources/logo.svg");
        if (!file.open(QIODevice::ReadOnly))
        {
            throw std::runtime_error("Failed to open logo.svg");
        }
        
        auto svgData = static_cast<QString>(file.readAll());
        file.close();

        // Change logo color in the SVG text
        svgData.replace("#ffffff", "#30261d", Qt::CaseInsensitive);
        
        // Create wrapper widget (needed to center-align the SVG)
        const auto wrapper = new QWidget(this);
        const auto wrapperLayout = new QHBoxLayout(wrapper);
        const auto logo = new QSvgWidget(wrapper);
        logo->load(svgData.toUtf8());
        logo->setFixedSize(260, 260);
        wrapperLayout->addWidget(logo);

        return wrapper;
    }
    
    QLabel* CreateLoadingLabel()
    {
        // Load font
        const auto id = QFontDatabase::addApplicationFont(":/Resources/Play-Regular.ttf");
        const auto family = QFontDatabase::applicationFontFamilies(id).at(0);
        auto font = QFont(family);
        font.setPixelSize(20);

        // Create label
        const auto label = new QLabel("Loading", this);
        label->setAlignment(Qt::AlignCenter);
        label->setFont(font);
        label->setText("Loading");
        label->setStyleSheet("color: #504030;");

        return label;
    }
    
    QProgressBar* CreateProgressBar()
    {
        const auto progress = new QProgressBar(this);
        progress->setRange(0, 0); // Uses indeterminate animation
        progress->setValue(0);
        progress->setTextVisible(false);
        progress->setFixedHeight(20);
        progress->setStyleSheet(R"(
            QProgressBar {
                background-color: #161310;
                border-radius: 0px;
            }
            QProgressBar::chunk {
                background-color: #30261d;
                border-radius: 0px;
            }
        )");

        return progress;
    }
};