using System;

public class Describe : Attribute
{
    private string desc;

    public string Description
    {
        get
        {
            return this.desc;
        }
    }

    public Describe( string description)
    {
        this.desc = description;
    }

    public Describe()
    {
    }
}
