// ReSharper disable CppDFAMemoryLeak (Qt widgets handle disposing their children)
#pragma once

#include <QApplication>
#include <QFile>
#include <QFontDatabase>
#include <QLabel>
#include <QProgressBar>
#include <QScreen>
#include <QVBoxLayout>
#include <QWidget>
#include <QSvgWidget>

class NativeSplashWindow
{
private:
    QWidget* window_;
    QLabel* loadingLabel_;

public:
    NativeSplashWindow() : window_(new QWidget())
    {
        // Set up window
        window_->setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
        window_->setWindowTitle("Flagrum");
        window_->setFixedSize(380, 450);
        window_->setStyleSheet("background: #181512;");
        window_->move(QApplication::primaryScreen()->geometry().center()
              - window_->rect().center());

        // Set up layout
        const auto layout = new QVBoxLayout(window_);
        layout->setContentsMargins(20, 30, 20, 20);
        layout->setSpacing(20);

        // Load logo file
        auto file = QFile(":/Resources/logo.svg");
        if (!file.open(QIODevice::ReadOnly))
        {
            throw std::runtime_error("Failed to open logo.svg");
        }
        
        auto svgData = static_cast<QString>(file.readAll());
        file.close();

        // Create logo widget
        const auto wrapper = new QWidget(window_); // Needed to align the logo to center
        const auto wrapperLayout = new QHBoxLayout(wrapper);
        wrapperLayout->setContentsMargins(0, 0, 0, 0);
        const auto logo = new QSvgWidget(wrapper);
        svgData.replace("#ffffff", "#30261d", Qt::CaseInsensitive);
        logo->load(svgData.toUtf8());
        logo->setFixedSize(260, 260);
        wrapperLayout->addWidget(logo);

        // Load font
        const auto id = QFontDatabase::addApplicationFont(":/Resources/Play-Regular.ttf");
        const auto family = QFontDatabase::applicationFontFamilies(id).at(0);
        auto font = QFont(family);
        font.setPixelSize(20);

        // Create loading text label
        loadingLabel_ = new QLabel(window_);
        loadingLabel_->setAlignment(Qt::AlignCenter);
        loadingLabel_->setFont(font);
        loadingLabel_->setText("Loading");
        loadingLabel_->setStyleSheet("color: #504030;");

        // Create the progress bar
        const auto progress = new QProgressBar(window_);
        progress->setRange(0, 0);
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

        // Add widgets to layout
        layout->addWidget(wrapper);
        layout->addStretch();
        layout->addWidget(loadingLabel_);
        layout->addWidget(progress);
    }
    
    ~NativeSplashWindow()
    {
        delete window_;
    }

    void Show() const
    {
        window_->show();
    }

    void Close() const
    {
        window_->close();
    }
    
    void SetLoadingText(const char* text) const
    {
        loadingLabel_->setText(text);
    }
};