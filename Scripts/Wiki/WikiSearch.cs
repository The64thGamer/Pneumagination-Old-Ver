using Godot;
using System;

public partial class WikiSearch : LineEdit
{
	[Export] Label articleTitle;
	[Export] RichTextLabel articleText;

	public override void _Ready()
	{
		TextSubmitted += Search;
		Search("Main Page");
	}

	void Search(String searchText)
	{
		ReleaseFocus();
		XmlParser parser = new XmlParser();
		parser.Open("res://Scripts/Wiki/WikiArticles.xml");
		
		string result = ParseXMLTitles(parser,searchText);
		if(result != "")
		{
			articleTitle.Text = searchText;
			articleText.Text = Tr(result);
			Text = string.Empty;
		}
	}

	public void URLClick(Variant meta)
	{
		Search(meta.ToString());
	}

	string ParseXMLTitles(XmlParser parser, string searchText)
	{
		while (parser.Read() != Error.FileEof)
		{
			if (parser.GetNodeType() == XmlParser.NodeType.Element)
			{
				for (int idx = 0; idx < parser.GetAttributeCount(); idx++)
				{
					if(parser.GetAttributeName(idx) == "title" && parser.GetAttributeValue(idx) == searchText)
					{
						return "WIKI_ARTICLE_" + parser.GetAttributeValue(idx).ToUpper().Replace(' ','_');
					}
				}
			}
		}
		return "";
	}
}
