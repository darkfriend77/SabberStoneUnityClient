using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using TMPro;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    public GameObject MessagePrefab;

    private bool _animateFlag;

    private float _showTimer;
    private float _colorFade;

    private GameObject _message;

    void Start()
    {
        _showTimer = 0.5f;
        _colorFade = 0;
    }

    void Update()
    {
        if (_animateFlag)
        {
            _showTimer -= Time.deltaTime;

            if (_showTimer <= 0)
            {
                _colorFade += 0.01f;

                foreach (var textMesh in GetComponentsInChildren<TextMeshProUGUI>())
                {
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, _colorFade);
                }

                if (_colorFade >= 1)
                {
                    _animateFlag = false;
                }

            }
        }
    }

    internal void GameInfoAnim(PlayState playState)
    {
        switch (playState)
        {
            case PlayState.WON:
                gameObject.SetActive(true);
                _message = Instantiate(MessagePrefab, gameObject.transform).gameObject;
                _message.GetComponent<TextMeshProUGUI>().text = "YOU WON!";
                _message.GetComponent<TextMeshProUGUI>().color = new Color(255, 0, 0, 0);
                _animateFlag = true;
                break;
            case PlayState.LOST:
                gameObject.SetActive(true);
                _message = Instantiate(MessagePrefab, gameObject.transform).gameObject;
                _message.GetComponent<TextMeshProUGUI>().text = "YOU LOST!";
                _message.GetComponent<TextMeshProUGUI>().color = new Color(0, 255, 0, 0);
                _animateFlag = true;
                break;
            case PlayState.TIED:
                gameObject.SetActive(true);
                _message = Instantiate(MessagePrefab, gameObject.transform).gameObject;
                _message.GetComponent<TextMeshProUGUI>().text = "TIED GAME!";
                _message.GetComponent<TextMeshProUGUI>().color = new Color(255,255,0,0);
                _animateFlag = true;
                break;
            case PlayState.CONCEDED:
            case PlayState.DISCONNECTED:
            case PlayState.INVALID:
            case PlayState.PLAYING:
            case PlayState.WINNING:
            case PlayState.LOSING:
            default:
                Debug.LogError($"Unhandled GameInfoAnim {playState}!!!");
                break;
        }
    }
}
