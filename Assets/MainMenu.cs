using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MainMenu : MonoBehaviour
{
    public Transform arrow;
    public List<Transform> arrowPositions;
    public GameObject specialThanksPanel;
    private int currentSelectIndex = 0;
    private void Update()
    {

        if (Input.anyKeyDown && specialThanksPanel.activeSelf)
        {
            HideSpecialThanks();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SelectUp();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SelectDown();
            }
            // enter or click to select current
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                Select();
            }
        }
    }

    private void SelectUp()
    {
        currentSelectIndex = Math.Max(currentSelectIndex - 1, 0);
        arrow.position = arrowPositions[currentSelectIndex].position;
    }

    private void SelectDown()
    {
        currentSelectIndex = Math.Min(currentSelectIndex + 1, arrowPositions.Count - 1);
        arrow.position = arrowPositions[currentSelectIndex].position;
    }

    private void Select()
    {
        Debug.Log(currentSelectIndex);
        switch (currentSelectIndex)
        {
            case 0:
                SceneManager.LoadScene("PaoPao Bar");
                break;
            case 2:
                ShowSpecialThanks();
                break;
            case 3:
#if UNITY_EDITOR
                // 停止播放模式
                EditorApplication.isPlaying = false;
                Debug.Log("Editor Play Mode Exited!"); // 打印日志以确认
#else
        // 如果在编译后的游戏中运行
        Application.Quit();
        Debug.Log("Application Quitted!"); // 在Build版本中，你可能看不到这个日志，因为程序已经退出了
#endif
                break;
        }
    }

    private void ShowSpecialThanks()
    {
        specialThanksPanel.SetActive(true);
    }

    private void HideSpecialThanks()
    {
        specialThanksPanel.SetActive(false);
    }
}
