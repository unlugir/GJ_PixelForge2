using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public enum CameraType
{
    TeamA,
    TeamB,
    Menu
}
public class CameraController : MonoBehaviour
{

    [SerializeField] private CinemachineCamera team1Camera;
    [SerializeField] private CinemachineCamera team2Camera;
    [SerializeField] private CinemachineCamera menuCamera;

    private Vector3 _team1CameraPosition;
    private Vector3 _team2CameraPosition;
    private bool _invertInput;
    private void Awake()
    {
        _team1CameraPosition = team1Camera.transform.position;
        _team2CameraPosition = team2Camera.transform.position;
    }

    public void ToggleCamera(CameraType type)
    {
        switch (type)
        {
            case CameraType.TeamA:
                team1Camera.Priority = 2;
                team2Camera.Priority = 0;
                menuCamera.Priority = 0;
                break;
            case CameraType.TeamB:
                team1Camera.Priority = 0;
                team2Camera.Priority = 2;
                menuCamera.Priority = 0;
                break;
            case CameraType.Menu:
                team1Camera.Priority = 0;
                team2Camera.Priority = 0;
                menuCamera.Priority = 2;
                break;
        }

        return;
    }

    public void ToggleTeamCamera(bool teamA)
    {
        _invertInput = !teamA;
        if (teamA)
            ToggleCamera(CameraType.TeamA);
        else{}
            ToggleCamera(CameraType.TeamB);
    }
    public void Move(bool teamA, Vector3 movement)
    {
        movement = _invertInput ? movement * -1 : movement;
        if (teamA)
            team1Camera.transform.position += movement;
        else
            team2Camera.transform.position += movement;
    }

    public void ResetCameras()
    {
        _invertInput = false;
        team1Camera.transform.position = _team1CameraPosition;
        team2Camera.transform.position = _team2CameraPosition;
    }
}
