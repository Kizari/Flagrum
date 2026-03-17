#pragma once

#include <QClipboard>
#include <QFileDialog>
#include <QIcon>
#include <QMessageBox>
#include <QObject>
#include <QSemaphore>
#include <QThread>

#include "MainWindow.hpp"
#include "SplashScreen.hpp"

using WebMessageReceivedCallback = void(*)(const char*);

/**
 * Types of native message boxes that this application supports.
 */
enum MessageBoxType
{
    Information,
    Warning,
    Critical
};

/**
 * Manages the minimal native UI needed to support the .NET/Blazor application.
 */
class ApplicationHost final : public QObject
{
    Q_OBJECT

private:
    QApplication* application_ = nullptr;
    int argc_ = 0;
    char* arg0_ = nullptr;
    char** argv_ = nullptr;

    SplashScreen* splashScreen_ = nullptr;
    MainWindow* mainWindow_ = nullptr;
    
public:
    /**
     * Initializes the QApplication and sets the default window icon.
     */
    ApplicationHost()
    {
        argc_ = 1;
        arg0_ = strdup("Flagrum");
        argv_ = new char*[2]{arg0_, nullptr};
        application_ = new QApplication(argc_, argv_);
        QApplication::setWindowIcon(QIcon(":/Resources/flagrum.ico"));
    }

    /**
     * Frees up native resources.
     */
    ~ApplicationHost() override
    {
        if (mainWindow_)
        {
            delete mainWindow_;
            mainWindow_ = nullptr;
        }
        
        if (splashScreen_)
        {
            delete splashScreen_;
            splashScreen_ = nullptr;
        }
        
        delete application_;
        application_ = nullptr;

        free(arg0_);
        arg0_ = nullptr;
    
        delete[] argv_;
        argv_ = nullptr;
    }

    /**
     * Runs the event loop indefinitely until `Exit` is called.
     * 
     * @return Exit code.
     */
    [[nodiscard]] int Run() const
    {
        return application_->exec();
    }

    /**
     * Closes any active splash screen and main window, then stops the event loop.
     * This causes any active calls on `Run` to return.
     * 
     * @param exitCode Exit code to return from `Run`.
     */
    void Exit(const int exitCode)
    {
        if (splashScreen_)
        {
            CloseSplash();
        }

        if (mainWindow_)
        {
            CloseMainWindow();
        }

        application_->processEvents();
        application_->exit(exitCode);
    }

    /**
     * Executes a function on the UI thread.
     * 
     * @param callback Function to execute.
     * @remarks Executes the callback directly if called from the UI thread.
     */
    void Invoke(std::function<void()> callback)
    {
        // Execute directly if already UI thread
        if (QThread::currentThread() == application_->thread())
        {
            callback();
            return;
        }

        // Dispatch to UI thread and wait for completion
        QSemaphore semaphore;
        QMetaObject::invokeMethod(this, [cb = std::move(callback), &semaphore]
        {
            cb();
            semaphore.release();
        }, Qt::QueuedConnection);
        
        semaphore.acquire();
    }

    /**
     * Creates and shows the splash screen.
     */
    void OpenSplash()
    {
        Invoke([&]
        {
            splashScreen_ = new SplashScreen();
            splashScreen_->show();
        });
    }

    /**
     * Closes and destroys the splash screen.
     */
    void CloseSplash()
    {
        Invoke([&]
        {
            splashScreen_->close();
            delete splashScreen_;
            splashScreen_ = nullptr;
        });
    }

    /**
     * Updates the text above the splash screen's loading bar.
     * 
     * @param text Text to display.
     */
    void SetSplashText(const char* text)
    {
        Invoke([&]
        {
            splashScreen_->SetLoadingText(text);
        });
    }

    /**
     * Creates and shows the main application window.
     */
    void OpenMainWindow()
    {
        Invoke([&]
        {
            mainWindow_ = new MainWindow();
            mainWindow_->show();
        });
    }

    /**
     * Closes and destroys the main application window.
     */
    void CloseMainWindow()
    {
        Invoke([&]
        {
            mainWindow_->close();
            delete mainWindow_;
            mainWindow_ = nullptr;
        });
    }

    /**
     * Sets the callback that will handle web messages sent to the host application by the embedded web view.
     * 
     * @param callback Web message handler.
     */
    void SetWebMessageHandler(const WebMessageReceivedCallback callback) const
    {
        mainWindow_->GetWebView()->SetWebMessageHandler(callback);
    }

    /**
     * Navigates the main window's embedded web view to a different page.
     * 
     * @param url URL to navigate to.
     */
    void NavigateWebView(const char* url)
    {
        Invoke([&]
        {
            mainWindow_->GetWebView()->setUrl(QUrl(url));
        });
    }

    /**
     * Executes a JavaScript snippet in the main window's embedded web view.
     * 
     * @param script JS code to execute.
     */
    void RunJavaScript(const char* script)
    {
        Invoke([&]
        {
            mainWindow_->GetWebView()->page()->runJavaScript(script);
        });
    }

    /**
     * Copies the given text into the system's clipboard.
     * 
     * @param text Text to copy.
     */
    void SetClipboardText(const char* text)
    {
        Invoke([&]
        {
            application_->clipboard()->setText(text);
        });
    }

    /**
     * Shows a native message box dialog.
     * 
     * @param title Message box title.
     * @param message Message box body text.
     * @param type Message box type.
     */
    void ShowMessageBox(const char* title, const char* message, const MessageBoxType type)
    {
        Invoke([&]
        {
            const auto parent = mainWindow_
                ? static_cast<QWidget*>(mainWindow_)
                : splashScreen_
                    ? static_cast<QWidget*>(splashScreen_)
                    : nullptr;

            switch (type)
            {
            case Information:
                QMessageBox::information(parent, title, message);
                break;
            case Warning:
                QMessageBox::warning(parent, title, message);
                break;
            case Critical:
                QMessageBox::critical(parent, title, message);
                break;
            }
        });
    }

    /**
     * Shows an open file dialog.
     * 
     * @param caption Dialog title.
     * @param directory Directory to show in the dialog when it first appears.
     * @param filter Qt-style file type filter string.
     * @param out Pointer to the buffer to store the resulting file path in.
     */
    void OpenFile(const char* caption, const char* directory, const char* filter, char* out)
    {
        Invoke([&]
        {
            const auto result = QFileDialog::getOpenFileName(mainWindow_, caption, directory, filter);
            const auto utf8 = result.toUtf8();
            memcpy(out, utf8.constData(), utf8.size());
            out[utf8.size()] = '\0';
        });
    }

    /**
     * Shows a save file dialog.
     * 
     * @param caption Dialog title.
     * @param directory Directory to show in the dialog when it first appears.
     * @param filter Qt-style file type filter string.
     * @param out Pointer to the buffer to store the resulting file path in.
     */
    void SaveFile(const char* caption, const char* directory, const char* filter, char* out)
    {
        Invoke([&]
        {
            const auto result = QFileDialog::getSaveFileName(mainWindow_, caption, directory, filter);
            const auto utf8 = result.toUtf8();
            memcpy(out, utf8.constData(), utf8.size());
            out[utf8.size()] = '\0';
        });
    }

    /**
     * Shows a directory selection dialog.
     * 
     * @param caption Dialog title.
     * @param directory Directory to show in the dialog when it first appears.
     * @param out Pointer to the buffer to store the resulting directory path in.
     */
    void OpenDirectory(const char* caption, const char* directory, char* out)
    {
        Invoke([&]
        {
            const auto result = QFileDialog::getExistingDirectory(mainWindow_, caption, directory);
            const auto utf8 = result.toUtf8();
            memcpy(out, utf8.constData(), utf8.size());
            out[utf8.size()] = '\0';
        });
    }
};