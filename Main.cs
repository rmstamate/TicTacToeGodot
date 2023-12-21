using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node
{
  [Export]
  public PackedScene circleScene;

  [Export]
  public PackedScene crossScene;

  [Export]
  public Marker2D nextPlayerMarkerPosition;

  [Export]
  public Label textLabel;

  [Export]
  private CanvasLayer gameOverMenu;

  private Sprite2D boardRef;
  private Vector2 boardSize;
  private Vector2 cellSize;
  private Vector2I gridPosition;
  private readonly int[,] gridData = new int[3, 3];
  private int player = 1;
  private Sprite2D nextPlayerMarker = null;
  private bool isGameOver = false;
  // private readonly List<Sprite2D> markerList = null;
  private readonly List<Sprite2D> markerList = new();

  public override void _Ready()
  {
    boardRef = GetNode<Sprite2D>("Board");

    if (boardRef != null)
    {
      boardSize = boardRef.Texture.GetSize();
      cellSize = boardSize / 3;
    }

    NewGame();
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseEvent && !isGameOver)
    {
      // GD.Print("mouse event type ", mouseEvent.ButtonIndex);

      if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed())
      {
        // GD.Print("mouse button event at ", mouseEvent.Position);

        if (mouseEvent.Position.X < boardSize.X && mouseEvent.Position.Y < boardSize.Y)
        {
          gridPosition = (Vector2I)(mouseEvent.Position / cellSize);
          GD.Print("Position", gridPosition);

          if (gridData[gridPosition.X, gridPosition.Y] == 0)
          {
            gridData[gridPosition.X, gridPosition.Y] = player;
            CreateMarker(player, gridPosition);
            player *= -1;
            UpdateMarker(player);
            CheckGameStatus();
          }

          GD.Print("Cell data is ", gridData[gridPosition.X, gridPosition.X]);
          string str = string.Join(',', gridData.Cast<int>());
          GD.Print("Game data: ", str);
        }
      }
    }
  }

  private void NewGame()
  {
    player = 1;
    isGameOver = false;
    gameOverMenu.Hide();
    UpdateMarker(player);

    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        gridData[i, j] = 0;
      }
    }

    if (markerList != null)
    {
      foreach (Sprite2D mark in markerList)
      {
        RemoveChild(mark);
      }

      markerList.Clear();
    }
  }

  private Vector2 GridPositionToWorldPosition(Vector2I grid)
  {
    Vector2 position = (grid * cellSize) + (cellSize / 2);
    return position;
  }

  private void CreateMarker(int player, Vector2I position)
  {
    Vector2 worldPosition = GridPositionToWorldPosition(position);
    Sprite2D marker = player == 1 ? (Sprite2D)circleScene.Instantiate() : (Sprite2D)crossScene.Instantiate();

    if (circleScene == null || crossScene == null) return;

    marker.Position = worldPosition;
    AddChild(marker);
    markerList.Add(marker);
    return;
  }

  private void UpdateMarker(int player)
  {
    Vector2 pos = nextPlayerMarkerPosition.Position;

    if (nextPlayerMarker != null)
    {
      RemoveChild(nextPlayerMarker);
    }

    if (player == 1)
    {
      nextPlayerMarker = (Sprite2D)circleScene.Instantiate();
      nextPlayerMarker.Position = pos;
      AddChild(nextPlayerMarker);
    }
    else
    {
      nextPlayerMarker = (Sprite2D)crossScene.Instantiate();
      nextPlayerMarker.Position = pos;
      AddChild(nextPlayerMarker);
    }
  }

  private void CheckGameStatus()
  {
    bool isBoardFull = !gridData.Cast<int>().Contains<int>(0);
    string message = player != 1 ? "Player 1 Victory!" : "Player 2 Victory!";

    for (int i = 0; i < 3; i++)
    {
      int rowSum = 0;
      int colSum = 0;

      for (int j = 0; j < 3; j++)
      {
        // Test for row sums
        rowSum += gridData[i, j];

        // Test for col sums
        colSum += gridData[j, i];

        // one row is the winner
        if (rowSum == 3 || rowSum == -3 || colSum == 3 || colSum == -3)
        {
          EndGame(message);
          return;
        }
      }

      rowSum = 0;
      colSum = 0;
    }

    // check for diagonal sum
    int leftDiagonalSum = gridData[0, 0] + gridData[1, 1] + gridData[2, 2];
    int rightDiagonalSum = gridData[2, 0] + gridData[1, 1] + gridData[0, 2];

    if (leftDiagonalSum == 3 || leftDiagonalSum == -3 || rightDiagonalSum == 3 || rightDiagonalSum == -3)
    {
      EndGame(message);
      return;
    }

    if (isBoardFull)
    {
      EndGame("Tie!");
      return;
    }

    isGameOver = false;
    return;
  }

  private void EndGame(string message)
  {
    isGameOver = true;
    textLabel.Text = message;
    gameOverMenu.Show();
    return;
  }

  private void OnButtonPressed()
  {
    NewGame();
  }
}
