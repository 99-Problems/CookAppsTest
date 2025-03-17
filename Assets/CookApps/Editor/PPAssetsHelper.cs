using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.Linq;
using Data;
using Data.Managers;

public class PPAssetsHelper : EditorWindow
{
    private string folderPath = "Assets/CookApps/Prefabs"; // �⺻ ���� ���
    private AddressableAssetGroup targetGroup; // Addressable �׷� ������ ���� ����
    private string uiPath = "Assets/CookApps/ui";

    // ������ â�� ���� ���� �޴� �׸� �߰�
    [MenuItem("CookAppsTest/Addressable Helper")]
    public static void ShowWindow()
    {
        GetWindow<PPAssetsHelper>("Addressable Helper");
    }

    // ������ â�� UI�� ǥ��
    private void OnGUI()
    {
        GUILayout.Label("Addressable Helper Tool", EditorStyles.boldLabel);

        // ���� ��� �Է� �ʵ�
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (targetGroup == null)
        {
            targetGroup = AddressableAssetSettingsDefaultObject.GetSettings(false).DefaultGroup;
        }
        // Addressable �׷� ���� �ʵ�
        targetGroup = (AddressableAssetGroup)EditorGUILayout.ObjectField(
            "Target Addressable Group",
            targetGroup,
            typeof(AddressableAssetGroup),
            false
        );

        // ���� �� ���ϵ��� Addressable�� �߰��ϴ� ��ư
        if (GUILayout.Button("Add Files to Addressables"))
        {
            AddFilesToAddressables();
            BuildAddressables();
        }
        if (GUILayout.Button("Addressables Build"))
        {
            BuildAddressables();
        }
    }

    // ���� �� ���ϵ��� Addressable�� �߰��ϴ� �Լ�
    private void AddFilesToAddressables()
    {
        if (string.IsNullOrEmpty(folderPath) || targetGroup == null)
        {
            Debug.LogError("Please specify a valid folder path and Addressable Group.");
            return;
        }

        // ���� ���� ��� ���ϰ� ������ GUID�� ������
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { folderPath });
        var UIGuids = AssetDatabase.FindAssets("", new[] { uiPath });
        UIGuids = UIGuids
            .Where(guid => !AssetDatabase.GUIDToAssetPath(guid).Contains($"{uiPath}/font"))
            .ToArray();
        var fontGuids = AssetDatabase.FindAssets("", new[] { $"{uiPath}/font" });

        if (assetGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {folderPath}");
            return;
        }
        else if (UIGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {uiPath}");
            return;
        }
        else if (fontGuids.Length == 0)
        {
            Debug.LogWarning($"No files found in folder: {uiPath}/font");
            return;
        }

        // Addressable ���� ��������
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        // ���� ���� ���ϵ��� Addressable�� ���
        foreach (string assetGUID in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;

                // JSON ���Ͽ� 'script' �� �߰�
                if (assetPath.EndsWith(".json"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Script.GetLabelString()}", true); // 'Script' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Script'");
                }

                // ���� �̸��� 'Popup'�� ���Ե� �����տ� 'Popup' �� �߰�
                else if (assetPath.EndsWith(".prefab") && assetPath.Contains("Popup"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Popup.GetLabelString()}", true); // 'Popup' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Popup'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Default.GetLabelString()}", true); // 'default' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address}");
                }
            }
        }
        foreach (string assetGUID in UIGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Mat'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.UI.GetLabelString()}", true);
                }
            }
        }

        foreach (string assetGUID in fontGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            // ������ �����ϰ� ���ϸ� �߰�
            if (!AssetDatabase.IsValidFolder(assetPath))
            {
                // ������ Addressable �׷쿡 �߰�
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(assetGUID, targetGroup);

                // ���� ��θ� �ּҷ� ���
                entry.address = assetPath;
                if (assetPath.EndsWith(".mat"))
                {
                    entry.SetLabel($"{Define.AssetLabel.Material.GetLabelString()}", true); // 'Mat' �� �߰�
                    Debug.Log($"Added {assetPath} to Addressables with address {entry.address} and label 'Mat'");
                }
                else
                {
                    entry.SetLabel($"{Define.AssetLabel.Font.GetLabelString()}", true);
                }
            }
        }

        // ���� ���� ����
        AssetDatabase.SaveAssets();
        Debug.Log("Addressables updated successfully.");
    }

    private void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent(); // Addressable ���� ����
        Debug.Log("Addressables build completed.");
    }
}

//public class AddressableBuildPreprocessor : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        AddressableAssetSettings.BuildPlayerContent();
//        Debug.Log("Addressables built before Unity build.");
//    }
//}
