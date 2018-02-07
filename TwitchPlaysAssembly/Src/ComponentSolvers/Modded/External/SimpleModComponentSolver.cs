﻿using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public class SimpleModComponentSolver : ComponentSolver
{
    public SimpleModComponentSolver(BombCommander bombCommander, BombComponent bombComponent, MethodInfo processMethod, Component commandComponent) :
        base(bombCommander, bombComponent)
	{
        ProcessMethod = processMethod;
        CommandComponent = commandComponent;
        modInfo = ComponentSolverFactory.GetModuleInfo(GetModuleType());
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (ProcessMethod == null)
        {
            DebugHelper.LogError("A declared TwitchPlays SimpleModComponentSolver process method is <null>, yet a component solver has been created; command invokation will not continue.");
            yield break;
        }

        KMSelectable[] selectableSequence = null;

        try
        {
            bool RegexValid = modInfo.validCommands == null;
            if (!RegexValid)
            {
                foreach (string regex in modInfo.validCommands)
                {
                    RegexValid = Regex.IsMatch(inputCommand, regex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    if (RegexValid)
                    {
                        break;
                    }
                }
            }
            if (!RegexValid)
                yield break;

            selectableSequence = (KMSelectable[])ProcessMethod.Invoke(CommandComponent, new object[] { inputCommand });
            if (selectableSequence == null || selectableSequence.Length == 0)
            {
                yield break;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogException(ex, string.Format("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", ProcessMethod.DeclaringType.FullName, ProcessMethod.Name));
            yield break;
        }

        if (!modInfo.DoesTheRightThing)
        {
            yield return "modsequence";
        }

        for(int selectableIndex = 0; selectableIndex < selectableSequence.Length; ++selectableIndex)
        {
            if (CoroutineCanceller.ShouldCancel)
            {
	            CoroutineCanceller.ResetCancel();
                yield break;
            }

            KMSelectable selectable = selectableSequence[selectableIndex];
            if (selectable == null)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            DoInteractionClick(selectable);
			yield return new WaitForSeconds(0.1f);
        }
    }

    private readonly MethodInfo ProcessMethod = null;
    private readonly Component CommandComponent = null;
}
