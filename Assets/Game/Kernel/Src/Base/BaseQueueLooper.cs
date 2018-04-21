using System.Collections.Generic;

public class BaseQueueLooper : IQueueLooper
{	
	public int QueueDataCount
	{
		get{return _queueDatas.Count;}
	}

	public bool HasQueueData
	{
		get{return QueueDataCount > 0;}
	}

	protected List<IQueueble> _queueDatas;

	public BaseQueueLooper() : this (new List<IQueueble>())
	{
		
	}

	public BaseQueueLooper(List<IQueueble> datas)
	{
		_queueDatas = datas;
	}

	public virtual IQueueble Peek()
	{
		return QueueDataCount > 0 ? _queueDatas[QueueDataCount - 1] : null;
	}

	public virtual IQueueble Dequeue(IQueueble data)
	{		
		if(null == data) _queueDatas.RemoveAt(0);
		else _queueDatas.Remove(data);
		if(null != data) data.OnDequeue();
		return data;
	}

	public virtual void Enqueue(IQueueble data, bool insert)
	{
		if(null == data) return;

		if(insert) _queueDatas.Insert(0, data);
		else _queueDatas.Add(data);
	}

	public virtual void Enqueue(List<IQueueble> datas, bool insert)
	{
		if(null == datas) return;

		if(insert) _queueDatas.InsertRange(0, datas);
		else _queueDatas.AddRange(datas);
	}

	public virtual bool UpdateLooper()
	{
		if(false == HasQueueData) return true;

		IQueueble queueData;
		queueData = Peek();
		if(null == queueData)
		{
			Dequeue(queueData);
			return false;
		}

		if(false == queueData.IsPlaying()) queueData.Play();

		if(queueData.IsDone() || false == queueData.IsBlockQueue()) Dequeue(queueData);

		return false;
	}
}
