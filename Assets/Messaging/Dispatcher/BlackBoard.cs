using System;
using System.Collections;
using System.Collections.Generic;
public class BlackBoard
{
	private struct WriteEntry
	{
		public string board;
		public string id;
		public object value;
		public WriteEntry(string board, string id, object value)
		{
			this.board = board;
			this.id = id;
			this.value = value;
		}
	}
	[Serializable]
	private class Board
	{
		private string name;
		private Hashtable boardContent;
		private int writeCount;
		private bool isRegistered;
		private Dictionary<string, Callback> registered;
		public Board(string name)
		{
			this.name = name;
			this.writeCount = 0;
			this.boardContent = new Hashtable();
			this.registered = new Dictionary<string, Callback>();
		}
		public void Write(string id, object value)
		{
			if (this.boardContent.Contains(id))
			{
				this.boardContent[id] = value;
			}
			else
			{
				this.boardContent.Add(id, value);
			}
			this.writeCount++;
		}
		public void WriteInstant(string id, object value)
		{
			if (this.boardContent.Contains(id))
			{
				this.boardContent[id] = value;
			}
			else
			{
				this.boardContent.Add(id, value);
			}
			this.writeCount++;
		}
		public bool Query(string id)
		{
			return this.boardContent.Contains(id);
		}
		public T Read<T>(string id)
		{
			if (this.isRegistered && this.registered.ContainsKey(id))
			{
				this.registered[id]();
			}
			if (this.boardContent.Contains(id))
			{
				return (T)((object)this.boardContent[id]);
			}
			return default(T);
		}
		public T UnsafeRead<T>(string id)
		{
			return (T)((object)this.boardContent[id]);
		}
		public void Clear()
		{
			this.boardContent.Clear();
			this.writeCount = 0;
		}
		public string[] GetAllFieldNames()
		{
			string[] array = new string[this.boardContent.Count];
			this.boardContent.Keys.CopyTo(array, 0);
			return array;
		}
		public string GetBoardName()
		{
			return this.name;
		}
		public int GetWriteCount()
		{
			return this.writeCount;
		}
		public void Register(string id, Callback callback)
		{
			this.isRegistered = true;
			this.registered.Add(id, callback);
		}
	}
	private static BlackBoard _instance = new BlackBoard();
	private Hashtable boards = new Hashtable();
	private Queue<BlackBoard.WriteEntry> pendingWrites = new Queue<BlackBoard.WriteEntry>();
	private List<string> boardNames = new List<string>();
	private List<string> result = new List<string>(1024);
	private bool isInitialCommitMade;
	private bool isGameStarted;
	private List<string> writtenIDs = new List<string>();
	private Hashtable cachedIDs = new Hashtable();
	private Hashtable cachedResults = new Hashtable();
	private static BlackBoard instance
	{
		get
		{
			if (BlackBoard._instance == null)
			{
				BlackBoard._instance = new BlackBoard();
			}
			return BlackBoard._instance;
		}
	}
	private BlackBoard()
	{
		DispatcherDaemon.Initialize();
	}
	public static void Write(string board, string id, object value)
	{
		BlackBoard.instance.pendingWrites.Enqueue(new BlackBoard.WriteEntry(board, id, value));
	}
	public static void WriteInstant(string board, string id, object value)
	{
		BlackBoard.WriteEntry writeEntry = new BlackBoard.WriteEntry(board, id, value);
		BlackBoard.Board board2;
		if (BlackBoard.instance.boards.Contains(writeEntry.board))
		{
			board2 = (BlackBoard.Board)BlackBoard.instance.boards[writeEntry.board];
		}
		else
		{
			board2 = new BlackBoard.Board(writeEntry.board);
			BlackBoard.instance.boards.Add(writeEntry.board, board2);
		}
		board2.WriteInstant(writeEntry.id, writeEntry.value);
		if (board2.GetWriteCount() == 1)
		{
			BlackBoard.instance.boardNames.Add(writeEntry.board);
		}
	}
	public static bool Query(string board, string id)
	{
		return BlackBoard.instance.boardNames.Contains(board) && ((BlackBoard.Board)BlackBoard.instance.boards[board]).Query(id);
	}
	public static T Read<T>(string board, string id)
	{
		if (!BlackBoard.instance.isInitialCommitMade || !BlackBoard.instance.isGameStarted)
		{
			BlackBoard.Commit();
		}
		if (BlackBoard.instance.boards.Contains(board))
		{
			return ((BlackBoard.Board)BlackBoard.instance.boards[board]).Read<T>(id);
		}
		return default(T);
	}
	public static T UnsafeRead<T>(string board, string id)
	{
		return ((BlackBoard.Board)BlackBoard.instance.boards[board]).UnsafeRead<T>(id);
	}
	public static void GameStarted()
	{
		BlackBoard.instance.isGameStarted = true;
	}
	public static void Commit()
	{
		BlackBoard.instance.isInitialCommitMade = true;
		while (BlackBoard.instance.pendingWrites.Count > 0)
		{
			BlackBoard.WriteEntry writeEntry = BlackBoard.instance.pendingWrites.Dequeue();
			BlackBoard.Board board;
			if (BlackBoard.instance.boards.Contains(writeEntry.board))
			{
				board = (BlackBoard.Board)BlackBoard.instance.boards[writeEntry.board];
			}
			else
			{
				board = new BlackBoard.Board(writeEntry.board);
				BlackBoard.instance.boards.Add(writeEntry.board, board);
			}
			board.Write(writeEntry.id, writeEntry.value);
			if (board.GetWriteCount() == 1)
			{
				BlackBoard.instance.boardNames.Add(writeEntry.board);
			}
			BlackBoard.instance.writtenIDs.Add(writeEntry.id);
		}
		BlackBoard.instance.UpdateCache();
		BlackBoard.instance.writtenIDs.Clear();
	}
	public static void Rollback()
	{
		BlackBoard.instance.pendingWrites.Clear();
	}
	public static void Clear()
	{
		BlackBoard.instance.pendingWrites.Clear();
		BlackBoard.instance.boards.Clear();
		BlackBoard.instance.boardNames.Clear();
	}
	public static void Clear(string board)
	{
		if (BlackBoard.instance.boards.Contains(board))
		{
			((BlackBoard.Board)BlackBoard.instance.boards[board]).Clear();
			BlackBoard.instance.boardNames.Remove(board);
		}
	}
	public static string[] GetAllFieldsFromBoard(string board)
	{
		BlackBoard.Board board2 = (BlackBoard.Board)BlackBoard.instance.boards[board];
		return board2.GetAllFieldNames();
	}
	public static string[] GetAllBoardsContaining(string id)
	{
		if (BlackBoard.instance.cachedIDs.Contains(id))
		{
			return (string[])BlackBoard.instance.cachedIDs[id];
		}
		BlackBoard.instance.result.Clear();
		for (int i = 0; i < BlackBoard.instance.boardNames.Count; i++)
		{
			if (BlackBoard.Query(BlackBoard.instance.boardNames[i], id))
			{
				BlackBoard.instance.result.Add(BlackBoard.instance.boardNames[i]);
			}
		}
		string[] value = BlackBoard.instance.result.ToArray();
		BlackBoard.instance.cachedIDs.Add(id, value);
		return value;
	}
	public static string[] GetAllBoardNames()
	{
		return BlackBoard.instance.boardNames.ToArray();
	}
	public static int GetWriteCountForBoard(string board)
	{
		if (BlackBoard.instance.boards.Contains(board))
		{
			return ((BlackBoard.Board)BlackBoard.instance.boards[board]).GetWriteCount();
		}
		return -1;
	}
	public static void Register(string board, string id, Callback callback)
	{
		if (BlackBoard.instance.boards.Contains(board))
		{
			((BlackBoard.Board)BlackBoard.instance.boards[board]).Register(id, callback);
			return;
		}
		BlackBoard.Board board2 = new BlackBoard.Board(board);
		board2.Register(id, callback);
		BlackBoard.instance.boards.Add(board, board2);
	}
	private void UpdateCache()
	{
		if (this.cachedIDs == null || this.cachedIDs.Count == 0 || this.writtenIDs == null)
		{
			return;
		}
		foreach (string current in this.writtenIDs)
		{
			if (this.cachedIDs.Contains(current))
			{
				this.cachedIDs.Remove(current);
			}
		}
	}
	public static void Save()
	{
		// Get all IsPersistent board names
		string[] allBoardsContaining = BlackBoard.GetAllBoardsContaining("IsPersistent");

		// List of all objects to be saved. <string>board, <string>field, <string|int|float|bool>value
		List<object> list = new List<object>();

		for (int i = 0; i < allBoardsContaining.Length; i++)
		{
			string[] fields = BlackBoard.GetAllFieldsFromBoard(allBoardsContaining[i]);

			for (int j = 0; j < fields.Length; j++)
			{
				object value = BlackBoard.Read<object>(allBoardsContaining[i], fields[j]);

				list.Add(allBoardsContaining[i]);
				list.Add(fields[j]);

				if (value.GetType() == typeof(bool))
					list.Add("__BOOLEAN:" + ((bool)value == true ? "true" : "false"));

				if (value.GetType() == typeof(int))
					list.Add("__INT32:" + value.ToString());
				
				if (value.GetType() == typeof(string))
					list.Add(value);
			}
		}

		BinarySaver.WriteBinFile(list.ToArray());
	}

	public static void Load()
	{
		object[] array = BinarySaver.ReadBinFile();

		if (array == null)
		{
			return;
		}

		for (int i = 0; i < array.Length - 3; i+=3)
		{
			string board = (string)array[i];
			string field = (string)array[i+1];
			object value = (object)array[i+2];

			if (value.GetType() == typeof(string))
			{

				if (((string)value).StartsWith("__BOOLEAN:"))
				{

					bool b = ((string)value).Contains("true") ? true : false;
					BlackBoard.WriteInstant(board, field, b);

					continue;
				}

				if (((string)value).StartsWith("__INT32:"))
				{

					int v = int.Parse(((string)value).Replace("__INT32:", ""));
					BlackBoard.WriteInstant(board, field, v);

					continue;
				}
			}

			BlackBoard.WriteInstant(board, field, value);
		}
	}
}
