using UnityEngine;

public static class GlobalSettings
{
    public static byte maxPlayersPerRoom = 10;
    public static int networkUpdateRate = 50;

    public static string gameVersion = "1";
    public static readonly string playerNamePrefKey = "playerName";
    public static readonly string vrEnabledPrefKey = "vrEnabled";
    public static readonly string avatarPrefKey = "avatarType";
    public static readonly string colorPrefKey = "shirtColor";

    public static string desktopResourcesPath = "Desktop/";
    public static string vrResourcesPath = "VR/";
    public static string shirtMaterialsPath = "Materials/ShirtMaterials/";
    public static LayerMask hideLayer = LayerMask.NameToLayer("Hide");

    public static readonly string[] defaultRooms = { "Project Meeting", "Captain Toad" };
    public static readonly string[] availableAvatars = { "Robot", "Ankith"};
}
