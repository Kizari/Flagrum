// ReSharper disable CppDFAMemoryLeak (Qt widgets handle disposing their children)
#pragma once

#include <QApplication>
#include <QFontDatabase>
#include <QHBoxLayout>
#include <QLabel>
#include <QProgressBar>
#include <QRandomGenerator>
#include <QScreen>
#include <QWidget>

/**
 * Simple splash screen window for the application.
 */
class SplashScreen final : public QWidget
{
private:
    static constexpr int WIDTH = 380;
    static constexpr int HEIGHT = 470;
    
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
        setWindowFlags(Qt::SplashScreen | Qt::FramelessWindowHint);
        setWindowTitle("Flagrum");
        setFixedSize(WIDTH, HEIGHT);
        move(QApplication::primaryScreen()->geometry().center() - rect().center());

        // Create layout
        const auto layout = new QVBoxLayout(this);
        layout->setContentsMargins(20, 30, 20, 20);
        layout->setSpacing(20);

        // Load font
        const auto id = QFontDatabase::addApplicationFont(":/Resources/Play-Regular.ttf");
        const auto family = QFontDatabase::applicationFontFamilies(id).at(0);
        auto font = QFont(family);
        font.setPixelSize(20);

        // Populate layout
        loadingLabel_ = CreateLoadingLabel(font);
        layout->addWidget(CreateLogo(), 0, Qt::AlignHCenter);
        layout->addStretch();
        layout->addWidget(loadingLabel_);
        layout->addWidget(CreateProgressBar());
        layout->addWidget(CreateAcknowledgement(font));
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

protected:
    /**
     * Applies a custom brushed texture to the background.
     */
    void paintEvent(QPaintEvent* event) override
    {
        auto painter = QPainter(this);
        constexpr auto base = QColor(28, 25, 23);
        static QImage texture = CreateBrushedTexture(base);
        painter.drawImage(0, 0, texture);
    }

private:
    /**
     * Creates a brushed aluminium style texture.
     * 
     * @param base Color to apply the brushed texture to.
     */
    static QImage CreateBrushedTexture(const QColor& base)
    {
        // Create random grayscale noise texture
        auto gray = QVector<uint8_t>(WIDTH * HEIGHT);
        for (auto i = 0; i < WIDTH * HEIGHT; ++i)
        {
            gray[i] = QRandomGenerator::global()->bounded(0, 255);
        }

        // Apply wide horizontal blur
        auto row = QVector<uint8_t>(WIDTH);

        for (auto pass = 0; pass < 3; ++pass)
        {
            for (auto y = 0; y < HEIGHT; ++y)
            {
                // Copy row into temporary buffer
                for (auto x = 0; x < WIDTH; ++x)
                {
                    row[x] = gray[y * WIDTH + x];
                }

                // Convolve with wide kernel
                for (auto x = 0; x < WIDTH; ++x)
                {
                    constexpr auto radius = 20;
                    auto sum = 0;
                    auto count = 0;

                    for (auto k = -radius; k <= radius; ++k)
                    {
                        const auto ix = x + k;
                        if (ix >= 0 && ix < WIDTH)
                        {
                            sum += row[ix];
                            count++;
                        }
                    }

                    gray[y * WIDTH + x] = static_cast<uint8_t>(sum / count);
                }
            }
        }

        // Allocate result
        auto result = QImage(WIDTH, HEIGHT, QImage::Format_ARGB32_Premultiplied);

        // Apply brush pattern to base color
        const auto hsl = base.toHsl();
        const auto h = hsl.hue();
        const auto s = hsl.saturation();
        const auto l = hsl.lightness();
        
        for (auto y = 0; y < HEIGHT; ++y)
        {
            const auto line = reinterpret_cast<QRgb*>(result.scanLine(y));
            
            for (auto x = 0; x < WIDTH; ++x)
            {
                const auto factor = static_cast<float>(gray[y * WIDTH + x]) / 255.0f;
                line[x] = QColor::fromHsl(h, s, l * factor + l / 3).rgba();
            }
        }

        return result;
    }
    
    QWidget* CreateLogo()
    {
        const auto image = QPixmap(":/Resources/logo.png");
        const auto label = new QLabel(this);
        label->setPixmap(image);
        label->setScaledContents(true);
        label->setFixedSize(280, 260);
        label->setAlignment(Qt::AlignCenter);
        label->setContentsMargins(20, 0, 0, 0);
        return label;
    }
    
    QLabel* CreateLoadingLabel(const QFont& font)
    {
        // Create label
        const auto label = new QLabel("Loading", this);
        label->setAlignment(Qt::AlignCenter);
        label->setFont(font);
        label->setText("Loading");
        label->setStyleSheet("color: #e1d9b7;");

        return label;
    }
    
    QProgressBar* CreateProgressBar()
    {
        const auto progress = new QProgressBar(this);
        progress->setRange(0, 0); // Uses indeterminate animation
        progress->setValue(0);
        progress->setTextVisible(false);
        progress->setFixedHeight(6);
        progress->setStyleSheet(R"(
            QProgressBar {
                border: none;
                background-color: rgb(16, 15, 12);
            }
            QProgressBar::chunk {
                background-color: #ada685;
            }
        )");

        return progress;
    }

    QLabel* CreateAcknowledgement(const QFont& font)
    {
        const auto label = new QLabel(this);
        label->setStyleSheet("font-size: 14px;");
        label->setFont(font);
        label->setText("Made with ♥ by Kizari");
        label->setAlignment(Qt::AlignCenter);
        label->setContentsMargins(0, 20, 0, 0);
        return label;
    }
};