using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private WorldGrid worldGrid;
    [SerializeField] private GridDrawer gridDrawer;
    [SerializeField] private PlayerController playerController;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(worldGrid).AsSelf();
        builder.RegisterInstance(gridDrawer).AsSelf();
        builder.RegisterInstance(playerController).AsSelf();
    }
}
