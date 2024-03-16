using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PF_Demo : Node
{
    [Export] LineEdit keyField;
    [Export] LineEdit valueField;

    [Export] Button storeButton;
    [Export] Button retrieveButton;

    [Export] Label warningText;
    [Export] Label emptyListLabel;

    [Export] string defaultKey;
    [Export] bool retrieveOnStart;

    [Export] Control listParent;
    //[Export] PF_Element elementTemplate;
    [Export] Button deleteAllButton;

    //Control listParent => elementTemplate.GetParent() as Control;

    [Export] PackedScene elementPrefab;

    PF_Element prefab => elementPrefab.Instantiate() as PF_Element;

    public override void _Ready()
    {
        warningText.Visible = false;
        storeButton.Pressed += Store;
        retrieveButton.Pressed += Retrieve;

        deleteAllButton.Pressed += () =>
        {
            PlayerPrefs.DeleteAll();
            BuildList();
        };

        keyField.Text = defaultKey;
        if (retrieveOnStart)
            Retrieve();

        BuildList();
    }

    void Store ()
    {
        warningText.Visible = false;

        if (string.IsNullOrEmpty(keyField.Text.Trim()))
            Warn("Key cannot be empty!");
        else
            PlayerPrefs.SetString(keyField.Text, valueField.Text);

        BuildList();
    }

    void Retrieve ()
    {
        warningText.Visible = false;

        if (string.IsNullOrEmpty(keyField.Text.Trim()))
            Warn("Key cannot be empty!");
        else
        {
            valueField.Text = PlayerPrefs.GetString(keyField.Text.Trim());
        }
    }

    void Warn (string msg)
    {
        warningText.Text = msg;
        warningText.Visible = true;
    }

    void BuildList ()
    {
        var pfs = PlayerPrefs.ListKeys.Clone() as string[];
        IEnumerable<PF_Element> elements = listParent.GetChildren().Where(c => ((Control)c) is PF_Element).Select(c => ((Control)c as PF_Element)).Where(c => c.Visible);

        emptyListLabel.Visible = pfs.Length == 0;

        for(int i = 0; i < Mathf.Max(pfs.Length, elements.Count()); i++)
        {
            if (i < elements.Count() && i < pfs.Length)
            {
                string val = PlayerPrefs.GetString(pfs[i]);

                elements.ElementAt(i).SetElement(pfs[i], val);
            }
            else if (i < elements.Count())
                elements.ElementAt(i).QueueFree();
            else if (i < pfs.Length)
            {
                var element = prefab;
                listParent.AddChild(element);
                element.Visible = true;
                string val = PlayerPrefs.GetString(pfs[i]);
                element.SetElement(pfs[i], val);
                int index = i;
                element.AssignDeleteAction(() =>
                {
                    PlayerPrefs.DeleteValue(PlayerPrefs.ListKeys[index]);
                    BuildList();
                });
                element.AssignMainButton((key) =>
                {
                    keyField.Text = key;
                });
            }
        }
    }

    void ClearList ()
    {
        var children = listParent.GetChildren();
        if (children?.Count > 0)
        {
            foreach (var child in children)
            {
                Control ctrl = (Control)child;
                if (ctrl.Visible)
                {
                    ctrl.QueueFree();
                }
            }
        }
    }
}
