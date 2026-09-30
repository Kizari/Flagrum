#pragma once

#include <cstring>
#include <string>
#include <unordered_map>

#define LSTRING(KEY) LocalizationService::GetInstance().GetString(KEY)

/**
 * Handles string localization for that native host.
 */
class LocalizationService
{
private:
    LocalizationService() = default;
    
    std::unordered_map<std::string, std::string> strings_;

public:
    /**
     * Gets the singleton localization service.
     * @return Singleton instance of this service.
     */
    static LocalizationService& GetInstance()
    {
        static LocalizationService instance;
        return instance;
    }

    /**
     * Populates the localization string map from a buffer.
     * 
     * @param stringCount Number of string pairs in the buffer.
     * @param stringBuffer Pointer to the buffer.
     */
    void Initialize(const int stringCount, char* stringBuffer)
    {
        strings_.reserve(stringCount);

        auto current = stringBuffer;
        
        for (auto i = 0; i < stringCount; ++i)
        {
            auto key = current;
            current += strlen(key) + 1;

            auto value = current;
            current += strlen(value) + 1;

            strings_.emplace(key, value);
        }
    }

    /**
     * Gets a localized string.
     * 
     * @param key Identifies the localized string to retrieve.
     * @return The requested localized string.
     */
    const std::string& GetString(const std::string& key) const
    {
        static const std::string empty;
        const auto it = strings_.find(key);
        return it == strings_.end() ? empty : it->second;
    }
};
