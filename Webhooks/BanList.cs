using System;
using System.Collections.Generic;

public class BanListItem
{
    public string Phrase { get; set; }
    public bool IsHardFilter { get; set; }
}
public class BanList
{
    private List<BanListItem> _phrases;
    public List<BanListItem> Phrases
    {
        get
        {
            if(_phrases == null)
            {
                _phrases = new List<BanListItem>();
            }
            return _phrases;
        } 
        set {
            _phrases = value;
        } 
    } 

    public string ListPhrases()
    {
        var res = "";
        foreach (var p in Phrases)
        {
            res += $"{p.Phrase},{p.IsHardFilter}\n";
        }
        return res;
    }
    public bool Contains(string phrase)
    {
        foreach (var p in Phrases)
        {
            if(p.Phrase == phrase) return true;
        }
        return false;
    }

    public void Add(string phrase, bool isHardFilter)
    {
        var newPhrase = new BanListItem() { IsHardFilter = isHardFilter, Phrase = phrase};
        Phrases.Add(newPhrase);
    }
    public BanListItem Find(string phrase)
    {
        BanListItem res = null;
        foreach (var p in Phrases)
        {
            if (p.Phrase == phrase)
            {
                res = p;
            }
        }
        return res;
    }
    public void Remove(string phrase)
    {
        var foundElement = Find(phrase);
        if(foundElement != null)
        {
            Phrases.Remove(foundElement);
        }
    }
}