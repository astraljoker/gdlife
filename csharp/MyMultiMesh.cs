using Godot;
using System;

public class MyMultiMesh : Godot.MultiMeshInstance
{
	private class Tile
	{
		public bool active;
		public bool next;
		public int life;
		public Tile[] others;
		
		public int GetValue()
		{
			var result = 0;
			for(int i = 0; i < others.Length; ++i)
			{
				if(others[i].active)
					++result;
			}
			
			return result;
		}
	}
	
	private class Game
	{
		public int w;
		public int h;
		public Tile[] tiles;
		
		private readonly Tile empty;
		
		public Game(int w, int h)
		{
			empty = new Tile();
			tiles = new Tile[w * h];
			this.w = w;
			this.h = h;
			
			for(int i = 0; i < w * h; ++i)
			{
				tiles[i] = new Tile();
			}
		}
		
		public Tile At(int x, int y)
		{
			if(x < 0 || x >= w || y < 0 || y >= h)
			{
				return empty;
			}
			
			return tiles[x + y * w];
		}
	}
	
	private Game game;
	private ulong started;
	
	public override void _Ready()
	{
		var r = new Random();
		var m = Multimesh;
		
		game = new Game(50, 50);
		
		m.ColorFormat = MultiMesh.ColorFormatEnum.Float;
		m.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		
		m.InstanceCount = game.w * game.h;
		
		{
			var i = 0;
			for(int y = 0; y < game.h; ++y)
			{
				for(int x = 0; x < game.w; ++x)
				{
					var t = game.tiles[i];
					t.others = new Tile[]
					{
						game.At(x - 1, y - 1),
						game.At(x, y - 1),
						game.At(x + 1, y - 1),
						game.At(x - 1, y),
						game.At(x + 1, y),
						game.At(x - 1, y + 1),
						game.At(x, y + 1),
						game.At(x + 1, y + 1),
					};
					
					var transform = new Transform(Basis.Identity, new Vector3(x, (r.Next() % 10) * 0.1f, y));
					m.SetInstanceTransform(i, transform);
					m.SetInstanceColor(i, new Color(0, 0, 0)); // 초기 색상 설정
					++i;
				}
			}
		}

		started = OS.GetSystemTimeMsecs();
	}
	
	private int counter;
	private int frame;
	
	public override void _Process(float delta)
	{
		if(++frame == 1000)
		{
			var elapsed = OS.GetSystemTimeMsecs() - started;
			GD.Print("Result: ", elapsed);
			GetTree().Quit();
			return;
		}
		
		{
			int i = 0;
			for (int y = 0; y < game.h; ++y)
			{
				for(int x = 0; x < game.w; ++x)
				{
					var t = game.tiles[i];
					var v = t.GetValue();
					
					if(t.active)
					{
						t.next = v == 2 || v == 3;
					}
					else
					{
						t.next = v == 3;
					}
					
					++i;
				}
			}
		}
		
		{
			for(int i = 0; i < game.tiles.Length; ++i)
			{
				var t = game.tiles[i];
				t.active = t.next;
				if(t.active)
				{
					t.life = 20;
				}
				else if(t.life > 0)
				{
					--t.life;
				}
			}
		}
		
		if(++counter == 10)
		{
			game.At(24, 25).active = true;
			game.At(25, 25).active = true;
			game.At(26, 25).active = true;
			game.At(25, 24).active = true;
			counter = 0;
		}
		
		{
			for(int i = 0; i < game.tiles.Length; ++i)
			{
				var t = game.tiles[i];
				if(t.active)
				{
					Multimesh.SetInstanceColor(i, new Color(1, 0, 0, 1));
				}
				else
				{
					Multimesh.SetInstanceColor(i, new Color(0, t.life / 50.0f, 0, 1));
				}
			}
		}
	}
}