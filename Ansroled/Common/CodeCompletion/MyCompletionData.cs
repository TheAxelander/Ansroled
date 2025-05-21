using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace Ansroled.Common.CodeCompletion;

public class MyCompletionData : ICompletionData
{
    public string Text { get; }
    
    public IImage? Image => null;

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content => _contentControl ??= BuildContentControl();

    public object Description => "Description for " + Text;

    public double Priority { get; } = 0;

    private Control? _contentControl;
    
    public MyCompletionData(string text)
    {
        Text = text;
    }
    
    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }

    private Control BuildContentControl()
    {
        TextBlock textBlock = new TextBlock();
        textBlock.Text = Text;
        textBlock.Margin = new Thickness(5);

        return textBlock;
    }
}