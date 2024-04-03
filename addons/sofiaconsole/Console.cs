// ReSharper disable once CheckNamespace

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace media.Laura.SofiaConsole;

using System.Collections.Generic;
using System.Reflection;
using Godot;

public partial class Console : Node
{
    public static Console Instance;
    public List<ConsoleCommandReference> Commands = new();
    private readonly List<string> _commandHistory = new();
    
    public bool Open;

    [Export] private CanvasLayer _consoleCanvas;
    [Export] private Panel _background;
    [Export] private Button _closeButton;
    [Export] private LineEdit _commandInput;
    [Export] private Button _commandSendButton;
    [Export] private ScrollContainer _historyScrollContainer;
    [Export] private VBoxContainer _historyContent;
    
    public override void _EnterTree()
    {
        Instance?.Free();
        Instance = this;
        
        GD.Print("[SofiaConsole] Initializing");

        _closeButton.Pressed += () => { SetConsole(false); };
        _commandSendButton.Pressed += () => { ProcessCommand(_commandInput.Text); };
        _commandInput.TextSubmitted += ProcessCommand;
        
        LoadCommands();
        
        Print("Pneumagination Debug Console [ Using SofiaConsole v1.2.0 ]", PrintType.Success);
        Space();
        
        GD.Print("[SofiaConsole] Done");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey { Pressed: true } eventKey)
        {
            // Close Console
            if (Open && eventKey.Keycode == Key.Escape)
            {
                SetConsole(false);
            }

            // Open Console
            if (InputMap.HasAction("toggle_console") && Input.IsActionPressed("toggle_console") || eventKey.Keycode == Key.F3)
            {
                ToggleConsole();
            }

            // Press Up to toggle between previous commands
            if (eventKey.Keycode == Key.Up && _commandInput.HasFocus() && _commandHistory.Count > 0)
            {
                var historyIndex = _commandHistory.FindIndex(x => x == _commandInput.Text);
                if (historyIndex == -1)
                {
                    historyIndex = _commandHistory.Count;
                }
                
                if (historyIndex == 0)
                {
                    historyIndex = _commandHistory.Count;
                }

                _commandInput.Text = _commandHistory[historyIndex - 1];
            }
        }

        if (@event is InputEventMouse mouseEvent && mouseEvent.IsPressed())
        {
            // TODO: Check if Mouse is not within Input/Button
            _commandInput.ReleaseFocus();
            _commandSendButton.ReleaseFocus();
        }
    }

    public void ToggleConsole()
    {
        SetConsole(!Open);
    }

    private void SetConsole(bool open)
    {
        Open = open;
        
        _consoleCanvas.Visible = Open;
        _background.MouseFilter = Open ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;

        if (Open)
        {
            _commandInput.GrabFocus();
            _historyScrollContainer.GetVScrollBar().Value = _historyScrollContainer.GetVScrollBar().MaxValue;
        }
    }

    private void ProcessCommand(string rawCommand)
    {
        if (rawCommand == "" || rawCommand == " ") return;
        
        GD.Print($"[SofiaConsole] Command: {rawCommand}");
        if (_commandHistory.Count > 50)
        {
            _commandHistory.RemoveAt(0);
        }
        _commandHistory.Add(rawCommand);

        Print($"> {rawCommand}", PrintType.Hint);

        // Split by spaces, unless in quotes
        string[] rawCommandSplit = Regex.Matches(rawCommand, @"[^\s""']+|""([^""]*)""|'([^']*)'").Select(m => m.Value)
            .ToArray();

        // Clear input
        _commandInput.Text = "";

        // Get command
        var commandAttribute = Commands.FirstOrDefault(x => x.Command == rawCommandSplit[0]);
        if (commandAttribute == null)
        {
            Print($"The command '{rawCommandSplit[0]}' does not exist.", PrintType.Error);
            return;
        }

        // Get Caller for Invoking Method
        var type = commandAttribute.Method.DeclaringType;
        if (type == null)
        {
            Print($"Could not execute command: {commandAttribute.Command}", PrintType.Error);
            return;
        }
        
        object instance;
        if (type.IsSubclassOf(typeof(GodotObject)))
        {
            // This is a Godot Object, find it or create a new instance
            instance = FindNodeByType(GetTree().Root, type) ?? Activator.CreateInstance(type);
        }
        else
        {
            // This is a generic class, create a new instance
            instance = Activator.CreateInstance(type);
        }

        if (instance == null)
        {
            Print($"Could not execute command: {commandAttribute.Command}", PrintType.Error);
            return;
        }
        
        // Fill parameters
        var paramInfos = commandAttribute.Method.GetParameters();
        var parameters = new object[paramInfos.Length];
        for (int i = 0; i < paramInfos.Length; i++)
        {
            if (rawCommandSplit.Length > i + 1 && rawCommandSplit[i + 1] != null)
            {
                parameters[i] = ConvertStringToType(rawCommandSplit[i + 1], paramInfos[i].ParameterType);
            }
            else
            {
                parameters[i] = null;
            }
        }
        
        // Invoke method
        commandAttribute.Method.Invoke(instance, parameters);
    }

    private void LoadCommands()
    {
        GD.Print("[SofiaConsole] Loading Commands");
        
        Commands = new List<ConsoleCommandReference>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (MethodInfo method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false);
                    
                    foreach (var attribute in attributes)
                    {
                        if (attribute is not ConsoleCommandAttribute consoleCommandAttribute) continue;
                        
                        if (Commands.FirstOrDefault(x => x.Command == consoleCommandAttribute.Command) != null)
                        {
                            throw new Exception($"[SofiaConsole] Duplicate console command: {consoleCommandAttribute.Command}");
                        }
                            
                        Commands.Add(new ConsoleCommandReference
                        {
                            Command = consoleCommandAttribute.Command,
                            Description = consoleCommandAttribute.Description,
                            Usage = consoleCommandAttribute.Usage,
                            Method = method
                        });
                        
                        GD.Print($"[SofiaConsole] Loaded command {consoleCommandAttribute.Command}");
                    }
                }
            }
        }
    }

    public async void Print(string text, PrintType type = PrintType.Default)
    {
        var newLabel = new Label
        {
            Text = text,
            Theme = new Theme
            {
                DefaultFontSize = 7
            }
        };

        Color labelColor;
        switch (type)
        {
            case PrintType.Default:
            default:
                labelColor = new Color("#efefef");
                break;
            case PrintType.Hint:
                labelColor = new Color("#666666");
                break;
            case PrintType.Warning:
                labelColor = new Color("#f39f24");
                break;
            case PrintType.Error:
                labelColor = new Color("#f91545");
                break;
            case PrintType.Success:
                labelColor = new Color("#00ff66");
                break;
        }

        newLabel.Modulate = labelColor;
        
        _historyContent.AddChild(newLabel);

        await ToSignal(GetTree(), "process_frame");

        // Scrolling Down
        _historyScrollContainer.GetVScrollBar().Value = _historyScrollContainer.GetVScrollBar().MaxValue;
    }

    public void ClearConsole()
    {
        foreach (var child in _historyContent.GetChildren())
        {
            child.Free();
        }
    }

    public void Space(int height = 1)
    {
        for (int i = 0; i < height; i++)
        {
            Print("");
        }
    }

    public enum PrintType
    {
        Default = 0,
        Hint = 1,
        Warning = 2,
        Error = 3,
        Success = 4,
    }
    
    private object ConvertStringToType(string input, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return input;
        }
        if (targetType == typeof(int))
        {
            return int.Parse(input);
        }
        if (targetType == typeof(float))
        {
            float.TryParse(input.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value);
            return value;
        }
        if (targetType == typeof(bool))
        {
            return bool.Parse(input);
        }
        throw new ArgumentException($"Unsupported type: {targetType}");
    }
    
    private static Node FindNodeByType(Node root, Type targetType)
    {
        return root.GetType() == targetType ? root : root.GetChildren().Select(child => FindNodeByType(child, targetType)).FirstOrDefault(foundNode => foundNode != null);
    }
}