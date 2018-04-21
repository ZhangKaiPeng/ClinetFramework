using System.Collections.Generic;

public interface IQueueLooper
{
	IQueueble Peek();

	IQueueble Dequeue(IQueueble data);

	void Enqueue(IQueueble data, bool insert);

	void Enqueue(List<IQueueble> data, bool insert);

	bool UpdateLooper();
}