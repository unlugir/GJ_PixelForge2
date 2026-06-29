using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private GridDrawer gridDrawer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private GameSettings gameSettings;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(gameSettings).AsSelf();
        builder.RegisterInstance(camera).AsSelf();
        builder.RegisterInstance(worldGrid).AsSelf();
        builder.RegisterInstance(gridDrawer).AsSelf();
        builder.RegisterInstance(playerController).AsSelf();
    }
}
