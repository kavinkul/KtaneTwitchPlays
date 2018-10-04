﻿using System;
using System.Collections;
using System.Linq;

public class MissionMessageResponder : MessageResponder
{
	private BombBinderCommander _bombBinderCommander = null;
	private FreeplayCommander _freeplayCommander = null;

	#region Unity Lifecycle
	private void OnEnable() =>
		// InputInterceptor.DisableInput();

		StartCoroutine(CheckForBombBinderAndFreeplayDevice());

	private void OnDisable()
	{
		StopAllCoroutines();

		_bombBinderCommander = BombBinderCommander.Instance = null;
		_freeplayCommander = FreeplayCommander.Instance = null;
	}
	#endregion

	#region Protected/Private Methods
	private IEnumerator CheckForBombBinderAndFreeplayDevice()
	{
		yield return null;

		SetupRoom setupRoom = (SetupRoom) SceneManager.Instance.CurrentRoom;
		_bombBinderCommander = new BombBinderCommander(setupRoom.BombBinder);
		_freeplayCommander = new FreeplayCommander(setupRoom.FreeplayDevice);
	}

	protected override void OnMessageReceived(Message message)
	{
		if (_bombBinderCommander == null)
		{
			return;
		}

		string text = message.Text;
		bool isWhisper = message.IsWhisper;
		string userNickName = message.UserNickName;

		if (!text.StartsWith("!") || text.Equals("!")) return;
		text = text.Substring(1).Trim();

		string[] split = text.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string textAfter = split.Skip(1).Join();
		switch (split[0])
		{
			case "binder":
				if ((TwitchPlaySettings.data.EnableMissionBinder && TwitchPlaySettings.data.EnableTwitchPlaysMode) || UserAccess.HasAccess(userNickName, AccessLevel.Admin, true) || TwitchPlaySettings.data.AnarchyMode)
				{
					_coroutineQueue.AddToQueue(_bombBinderCommander.RespondToCommand(new Message(userNickName, null, textAfter, isWhisper), null));
				}
				else
				{
					IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.MissionBinderDisabled, userNickName), userNickName, !isWhisper);
				}
				break;
			case "freeplay":
				if ((TwitchPlaySettings.data.EnableFreeplayBriefcase && TwitchPlaySettings.data.EnableTwitchPlaysMode) || UserAccess.HasAccess(userNickName, AccessLevel.Admin, true) || TwitchPlaySettings.data.AnarchyMode)
				{
					_coroutineQueue.AddToQueue(_freeplayCommander.RespondToCommand(new Message(userNickName, null, textAfter, isWhisper), null));
				}
				else
				{
					IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.FreePlayDisabled, userNickName), userNickName, !isWhisper);
				}
				break;
			default:
				_coroutineQueue.AddToQueue(TPElevatorSwitch.Instance.ProcessElevatorCommand(split));
				break;
		}
	}
	#endregion
}
