using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceNodeDataItem : Object
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.SequenceNode";
    private Dictionary<string, int> runtimeActivatedCountTemplapteTray_;
    private int runtimeActivatedGeneration_ = -1;
    private Dictionary<string, string> runtimeDebuggerTextTemplapteTray_;

    public SequenceNodeDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        createEditorOnlySequenceNodeItem();
    }

    private void createEditorOnlySequenceNodeItem()
    {
        createCommentItem();
    }

    public int GetRuntimeActivatecCount(int generation, string templateTrayPath = "")
    {
        return runtimeActivatedGeneration_ == generation && runtimeActivatedCountTemplapteTray_ != null &&
               runtimeActivatedCountTemplapteTray_.ContainsKey(templateTrayPath)
            ? runtimeActivatedCountTemplapteTray_[templateTrayPath]
            : 0;
    }

    public void SetRuntimeActivatecCount(int count, int generation, string templateTrayPath = "")
    {
        if (runtimeActivatedCountTemplapteTray_ == null)
        {
            runtimeActivatedCountTemplapteTray_ = new Dictionary<string, int>();
        }

        if (runtimeActivatedGeneration_ != generation)
        {
            runtimeActivatedCountTemplapteTray_.Clear();
        }

        runtimeActivatedCountTemplapteTray_[templateTrayPath] = count;
        runtimeActivatedGeneration_ = generation;
    }

    public string GetRuntimeDebuggerText(int generation, string templateTrayPath = "")
    {
        return runtimeActivatedGeneration_ == generation && runtimeDebuggerTextTemplapteTray_ != null &&
               runtimeDebuggerTextTemplapteTray_.ContainsKey(templateTrayPath)
            ? runtimeDebuggerTextTemplapteTray_[templateTrayPath]
            : "";
    }

    public void SetRuntimeDebuggerText(string text, int generation, string templateTrayPath = "")
    {
        if (runtimeDebuggerTextTemplapteTray_ == null)
        {
            runtimeDebuggerTextTemplapteTray_ = new Dictionary<string, string>();
        }

        if (runtimeActivatedGeneration_ != generation)
        {
            runtimeDebuggerTextTemplapteTray_.Clear();
        }

        runtimeDebuggerTextTemplapteTray_[templateTrayPath] = text;
        runtimeActivatedGeneration_ = generation;
    }
}