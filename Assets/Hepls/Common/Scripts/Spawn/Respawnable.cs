namespace MoreMountains.TopDownEngine
{
	public interface Respawnable
	{
		void OnPlayerRespawn(CheckPoint checkpoint, Character player);
	}
}