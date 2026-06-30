using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private GameFlow gameFlow;
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private GridDrawer gridDrawer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIController uiController;
    [SerializeField] private GameSettings gameSettings;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(gameSettings).AsSelf();
        builder.RegisterInstance(cameraController).AsSelf();
        builder.RegisterInstance(uiController).AsSelf();
        builder.RegisterInstance(worldGrid).AsSelf();
        builder.RegisterInstance(gridDrawer).AsSelf();
        builder.RegisterInstance(playerController).AsSelf();
        builder.RegisterInstance(gameFlow).AsSelf();
    }
}
