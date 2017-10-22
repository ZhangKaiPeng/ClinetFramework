
public interface IQueueble
{
	bool IsPlaying();

	bool IsDone();

	bool IsBlockQueue();

	void SetBlockQueue(bool value);

	void Play();

	void OnDequeue();
}
