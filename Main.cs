using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using Steamworks;
using CellMenu;
using HarmonyLib;

namespace AllAchievements;

[BepInPlugin("AllAchievements", "AllAchievements", "11.45.14")]
public class EntryPoint : BasePlugin
{
	public override void Load()
	{
		Log.LogWarning(
			$"ATTENTION! " +
			$"This plugin will trying to completing all the achievements of GTFO after seconds. " +
			$"It cannot be cancelled unless you shutdown the game right now."
			);
		Main.logger = Log;
		ClassInjector.RegisterTypeInIl2Cpp<Main>();
		new Harmony("AllAchievements").PatchAll();
	}
}

[HarmonyPatch(typeof(CM_PageRundown_New), "PlaceRundown")]
internal static class MainInjection
{
	public static void Postfix()
	{
		if (!MainInjection.isPatched)
		{
			GameObject game_object = new();
			game_object.AddComponent<Main>();
			UnityEngine.Object.DontDestroyOnLoad(game_object);
			MainInjection.isPatched = true;
		}
	}
	private static bool isPatched = false;
}

public class Main : MonoBehaviour
{
	public static ManualLogSource? logger;
	private int last_countdown_number = 0;
	private float countdown = 20.0f;
	private void Update()
	{
		if (countdown > 0.0f && logger != null)
		{
			countdown -= Time.deltaTime;
			if (countdown <= 0.0f)
			{
				logger.Log(BepInEx.Logging.LogLevel.Message, "Start.");
				try
				{
					uint ach_num = SteamUserStats.GetNumAchievements();
					logger.Log(
							BepInEx.Logging.LogLevel.Message,
							String.Format(
								"{0} achievements has been found.",
								Convert.ToInt32(ach_num)
								)
							);
					for (uint i = 0; i < ach_num; i++)
					{
						string name = SteamUserStats.GetAchievementName(i);
						SteamUserStats.SetAchievement(name);
						logger.Log(
							BepInEx.Logging.LogLevel.Message,
							String.Format(
								"{0} / {1} : " + name + " = " + SteamUserStats.GetAchievementDisplayAttribute(name, "name"),
								Convert.ToInt32(i + 1), ach_num
								)
							);
					}
					SteamUserStats.StoreStats();
				}
				catch (Exception ex)
				{
					logger.LogError(ex);
					throw;
				}
				logger.Log(BepInEx.Logging.LogLevel.Message, "End.");
				return;
			}
			int countdown_number = Convert.ToInt32(countdown);
			if (countdown_number != last_countdown_number)
			{
				logger.Log(
				BepInEx.Logging.LogLevel.Warning,
				String.Format(
					"ATTENTION! " +
					"This plugin will trying to completing all the achievements of GTFO after {0} seconds. " +
					"It cannot be cancelled unless you shutdown the game right now.",
					countdown_number
					)
				);
				last_countdown_number = countdown_number;
			}
		}
	}
}