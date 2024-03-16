/// PlayerPrefs
/// by MarcWerk
/// October 2023 (last modified: Janurary 2024)
/// 
/// A quick way to store and retrieve small amounts of data.
/// Uses Windows registry when running in windows, otherwise it uses a collection of files in local storage
/// 
/// Change Log:
///  (Jan. 2024)
///  - Fixed a case where Registry.CurrentUser can be null until something is written to it.
///  - Made fallback to using filesystem registry if despite above fix Registry.CurrentUser is still null

using Godot;
using System;

#if GODOT_WINDOWS
using Microsoft.Win32;
#endif

public static class PlayerPrefs
{
    static string registyPath => $@"Software\{AppName}";

#if GODOT_WINDOWS

    static RegistryKey _currentUser;

    static RegistryKey CurrentUser
    {
        get
        {
            if(_currentUser == null)
                CreateCurrentUser();
            return _currentUser;
        }
    }

    static bool useFileSystemFallback = false;

    static void CreateCurrentUser ()
    {
#pragma warning disable CA1416
        try
        {
            _currentUser = Registry.CurrentUser.OpenSubKey(registyPath, true);
        } catch
        {
            GD.Print("Creating Registry.CurrentUser...");
            try
            {
                _currentUser = Registry.CurrentUser.CreateSubKey(registyPath, true);
                _currentUser.SetValue(AppName, true);
                GD.Print("Created Registry.CurrentUser");
            } catch (Exception e)
            {
                useFileSystemFallback = true;
                _currentUser = null;
                GD.PrintErr("Failed to create Registry.CurrentUser, using filesystem callback");
            }
        }
#pragma warning restore CA1416
    }

#endif

    static string AppName
    {
        get
        {
            return ProjectSettings.GetSetting("application/config/name", "Godot Project").ToString();
        }
    }

    public static string[] ListKeys
    {
        get
        {
#if GODOT_WINDOWS

            if(useFileSystemFallback || CurrentUser == null)
            {
                CheckDir();
                DirAccess dir = DirAccess.Open(filePath);
                return dir.GetFiles() ?? new string[0];
            }

            try
            {
#pragma warning disable CA1416
                string[] subKeys = CurrentUser.GetValueNames();
#pragma warning restore CA1416
                return subKeys;
            } catch (Exception e)
            {
                GD.PrintErr(e);
                return new string[0];
            }
#else
            CheckDir();
            DirAccess dir = DirAccess.Open(filePath);
            return dir.GetFiles() ?? new string[0];   
#endif
        }
    }


    public static void SetValue<T> (string key, T value)
    {
#if GODOT_WINDOWS

        if(useFileSystemFallback || CurrentUser == null)
        {
            SetLocal(key, value.ToString());
            return;
        }

        try
        {
#pragma warning disable CA1416
            CurrentUser.SetValue(key, value);
#pragma warning restore CA1416
        }
        catch (Exception e) 
        {
            GD.PrintErr(e);
        }
#else
        SetLocal(key, value.ToString());
#endif
    }

    public static T GetValue<T> (string key, T defaultValue = default(T))
    {
        try
        {
            object val;
#if GODOT_WINDOWS

            if(useFileSystemFallback || CurrentUser == null)
            {
                val = GetLocal(key);
            } else 
            { 
#pragma warning disable CA1416
                val = CurrentUser.GetValue(key);
#pragma warning restore CA1416
#else
                val = GetLocal(key);
#endif
            }

            if (val == null)
            {
                GD.PrintErr($"Player Prefs Key {key} not found!");
                return defaultValue;
            } else
            {
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(val, typeof(T));
                }
                else
                {
                    GD.PrintErr($"Cannot store data of type {typeof(T).Name} in PlayerPrefs");
                    return defaultValue;
                }
            }


        } catch (Exception e)
        {
            GD.PrintErr(e);
            return defaultValue;
        }
    }

    public static void DeleteValue (string key)
    {
#if GODOT_WINDOWS

        if(useFileSystemFallback || CurrentUser == null)
        {
            DeleteLocal(key);
            return;
        }

        try
        {
#pragma warning disable CA1416
            RegistryKey registryKey = CurrentUser;
#pragma warning restore CA1416

            if (registryKey != null)
#pragma warning disable CA1416
                registryKey.DeleteValue(key);
#pragma warning restore CA1416
            else
                GD.Print($"Registry key '{key}' not found.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error deleting key: {ex.Message}");
        }
#else
        DeleteLocal(key);
#endif
    }

    public static void DeleteAll ()
    {
        var listCopy = ListKeys.Clone() as string[];
        foreach(var key in listCopy)
            DeleteValue(key);
    }

    public static void SetBool (string key, bool value) => SetValue(key, value);
    public static bool GetBool (string key, bool defaultValue = false) => GetValue(key, defaultValue);

    public static void SetInt (string key, int value) => SetValue(key, value);
    public static int GetInt (string key, int defaultValue = 0) => GetValue(key, defaultValue);

    public static void SetString (string key, string value) => SetValue(key, value);
    public static string GetString (string key, string defaultValue = "") => GetValue(key, defaultValue);

    public static void SetFloat (string key, float value) => SetValue(key, value);
    public static float GetFloat (string key, float defaultValue = 0f) => GetValue(key, defaultValue);

    #region Local Storage
    public static string filePath => $"user://playerprefs/";
    static void CheckDir()
    {
        DirAccess root = DirAccess.Open("user://");
        if (!root.DirExists("playerprefs"))
            root.MakeDir("playerprefs");
    }

    static void SetLocal(string key, string value)
    {
        CheckDir();

        string path = filePath + "/" + key;

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(value.ToString());
        file.Close();
    }

    static string GetLocal(string key)
    {
        CheckDir();

        string path = filePath + "/" + key;

        if (FileAccess.FileExists(path))
        {
            FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string text = file.GetAsText();
            file.Close();
            return text;
        }
        return null;
    }

    static void DeleteLocal(string key)
    {
        CheckDir();

        string path = filePath + "/" + key;
        if (FileAccess.FileExists(path))
        {
            DirAccess dir = DirAccess.Open(filePath);
            dir.Remove(key);
        }
    }
    #endregion
}
