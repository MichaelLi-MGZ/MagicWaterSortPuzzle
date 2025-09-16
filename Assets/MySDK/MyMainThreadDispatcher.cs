using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MyMainThreadDispatcher : MonoBehaviour
{
	public static MyMainThreadDispatcher instance = null;
	static readonly Queue<Action> mQueue = new Queue<Action>();

	public void Update()
	{
		while (true) {
			Action action = null;
			lock (mQueue) {
				if (mQueue.Count == 0)
					break; // exit loop
				action = mQueue.Dequeue();
			}
			try {
				action.Invoke();
			}
			catch (Exception e) {
				Debug.LogError("mysdk MyMainThreadDispatcher::Update() action exception: " + e.Message);
			}
		}
	}

	public void Enqueue(Action action)
	{
		lock (mQueue) {
			mQueue.Enqueue(() => {
				StartCoroutine(MyAction(action));
			});
		}
	}

	private IEnumerator MyAction(Action action)
	{
		action();
		yield return null;
	}

	private void Awake()
    {
		if (instance == null)
			instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
}
