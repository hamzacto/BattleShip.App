using BlazorBootstrap;
using BattleShip.API.Model;

namespace BattleShip.Client
{
	public class GameState
	{
		public string GameId { get; set; }
		public string CurrentPlayerId { get; set; } // Le joueur actuel
		public string OpponentPlayerId { get; set; } // Joueur opposé (utile pour le multijoueur)
		public List<List<char>> PlayerGrid { get; set; }
		public List<List<char>> OpponentGrid { get; set; }
		//public bool?[][] MaskedPlayerGrid { get; set; }
		//spublic bool?[][] MaskedOppenentGrid { get; set; }
		public List<ShipStatus> PlayerShips { get; set; } // Les bateaux du joueur
		public List<ShipStatus> OpponentShips { get; set; } // Les bateaux de l'adversaire
		public BattleHistory History { get; set; } // Historique des coups
		public bool IsGameOver { get; private set; } // Indique si le jeu est terminé
		public string? WinnerId { get; set; } // Identifiant du gagnant 
		public int GameMode { get; set; }
		public int IaLvl { get; set; }
		public int PVE { get; set; }

		private Dictionary<(int X, int Y), Guid> gridToShipMap = new Dictionary<(int X, int Y), Guid>();

		// Initialisation des grilles

		public GameState(string playerId, string opponentPlayerId)
		{
			GameId = Guid.NewGuid().ToString();
			CurrentPlayerId = playerId;
			OpponentPlayerId = opponentPlayerId;

			// Initialize PlayerGrid and OpponentGrid as List<List<char>>
			PlayerGrid = InitializeGrid(); // Initialize the player's grid
			OpponentGrid = InitializeGrid(); // Initialize the opponent's grid

			PlayerShips = InitializeShips(); // Initialize the player's ships
			OpponentShips = InitializeShips(); // Initialize the opponent's ships

			// Place ships on both grids
			PlaceShipsOnGrid(PlayerShips, PlayerGrid);
			PlaceShipsOnGrid(OpponentShips, OpponentGrid);

			History = new BattleHistory
			{
				GameId = GameId, // Assign the GameId
				Moves = new List<BattleMove>()
			}; // Initialize the history
			IsGameOver = false;
			WinnerId = null;
		}

		public GameState() // Parameterless constructor for EF Core
		{
			PlayerGrid = new List<List<char>>();
			OpponentGrid = new List<List<char>>();
			PlayerShips = new List<ShipStatus>();
			OpponentShips = new List<ShipStatus>();
			History = new BattleHistory();
		}


		private List<List<char>> InitializeGrid()
		{
			// Create a 10x10 grid initialized with '\0'
			return Enumerable.Range(0, 10)
							 .Select(_ => Enumerable.Repeat('\0', 10).ToList())
							 .ToList();
		}

		private void PlaceShipsOnGrid(List<ShipStatus> ships, List<List<char>> grid)
		{
			Random random = new Random();

			foreach (var ship in ships)
			{
				bool placed = false;

				while (!placed)
				{
					bool isHorizontal = random.Next(0, 2) == 0;
					int startX = random.Next(0, 10);
					int startY = random.Next(0, 10);

					if (CanPlaceShip(grid, ship, startX, startY, isHorizontal))
					{
						for (int i = 0; i < ship.Size; i++)
						{
							int x = startX + (isHorizontal ? i : 0);
							int y = startY + (isHorizontal ? 0 : i);

							grid[x][y] = ship.ShipType; // Mark the ship on the grid with its ShipType
							gridToShipMap[(x, y)] = ship.ShipId; // Map position to ShipId
						}
						placed = true;
					}
				}
			}
		}

		private bool CanPlaceShip(List<List<char>> grid, ShipStatus ship, int startX, int startY, bool isHorizontal)
		{
			if (isHorizontal)
			{
				if (startX + ship.Size > 10)
					return false;
			}
			else
			{
				if (startY + ship.Size > 10)
					return false;
			}

			for (int i = 0; i < ship.Size; i++)
			{
				int x = startX + (isHorizontal ? i : 0);
				int y = startY + (isHorizontal ? 0 : i);

				if (grid[x][y] != '\0') // '\0' represents an empty space
					return false;
			}

			return true;
		}

		private List<ShipStatus> InitializeShips()
		{
			return new List<ShipStatus>
		{
			new ShipStatus { ShipType = 'A', ShipName = "Porte-avions", Size = 5 },
			new ShipStatus { ShipType = 'B', ShipName = "Cuirassé", Size = 4 },
			new ShipStatus { ShipType = 'C', ShipName = "Croiseur", Size = 3 },
			new ShipStatus { ShipType = 'S', ShipName = "Sous-marin", Size = 3 },
			new ShipStatus { ShipType = 'D', ShipName = "Destroyer", Size = 2 }
		};
		}


	}
}
