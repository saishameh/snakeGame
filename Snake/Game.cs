using Snake;

// Game configuration
const int SPEED_INCREASE_THRESHOLD = 5;
const int SPEED_INCREASE_AMOUNT = 5;
const int POINTS_PER_APPLE = 10;

// Game state variables
Random random = new Random();
Coord gridDimensions = new Coord(50, 20);
Coord snakePos = new Coord(10, 1);
Direction movementDirection = Direction.Down;
Direction lastMovementDirection = Direction.Down;
List<Coord> snakePosHistory = new List<Coord>();

int tailLength = 1;
int frameDelayMilli = 100;
int score = 0;
int highScore = 0;
bool gameOver = false;
bool gameActive = true;

// Visual appearance
ConsoleColor snakeHeadColor = ConsoleColor.Green;
ConsoleColor snakeBodyColor = ConsoleColor.DarkGreen;
ConsoleColor appleColor = ConsoleColor.Red;
ConsoleColor wallColor = ConsoleColor.Gray;
ConsoleColor textColor = ConsoleColor.Yellow;

// Initialize the game
Console.CursorVisible = false;  // Hide the cursor for cleaner display
Coord applePos = GenerateApplePosition();
DisplayWelcomeScreen();

// Main game loop
while (gameActive)
{
    // Clear screen and update display
    Console.Clear();
    DisplayGameInfo();
    
    if (!gameOver)
    {
        // Move snake
        lastMovementDirection = movementDirection;
        snakePos.ApplyMovementDirection(movementDirection);
    }
    
    // Render game elements
    RenderGame();
    
    // Handle game over conditions
    if (!gameOver && IsGameOver())
    {
        HandleGameOver();
        continue;  // Skip the rest of the loop and start the next iteration
    }
    
    // Check if snake has picked up apple
    if (!gameOver && snakePos.Equals(applePos))
    {
        HandleApplePickup();
    }
    
    // Update snake body
    if (!gameOver)
    {
        UpdateSnakeBody();
    }
    
    // Process player input and delay next frame
    ProcessInputAndDelay();
}

// End program properly
Console.CursorVisible = true;  // Restore cursor visibility

// Game logic methods
bool IsGameOver()
{
    // Hit wall
    if (snakePos.X <= 0 || snakePos.Y <= 0 || 
        snakePos.X >= gridDimensions.X - 1 || 
        snakePos.Y >= gridDimensions.Y - 1)
    {
        return true;
    }
    
    // Hit self
    if (snakePosHistory.Contains(snakePos))
    {
        return true;
    }
    
    return false;
}

void HandleGameOver()
{
    gameOver = true;
    
    // Update high score
    if (score > highScore)
        highScore = score;
        
    // Display game over screen
    DisplayGameOverScreen();
    
    // Wait for restart or exit input
    bool waitingForInput = true;
    while (waitingForInput && gameActive)
    {
        if (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            
            if (key == ConsoleKey.R)
            {
                ResetGame();
                gameOver = false;
                waitingForInput = false;
            }
            else if (key == ConsoleKey.Escape)
            {
                gameActive = false;
                waitingForInput = false;
            }
        }
        Thread.Sleep(50);  // Small delay to prevent CPU hogging
    }
}

void HandleApplePickup()
{
    tailLength++;
    score += POINTS_PER_APPLE;
    
    // Increase speed every SPEED_INCREASE_THRESHOLD apples
    if (tailLength % SPEED_INCREASE_THRESHOLD == 0 && frameDelayMilli > 50)
    {
        frameDelayMilli -= SPEED_INCREASE_AMOUNT;
    }
    
    // Generate new apple position
    applePos = GenerateApplePosition();

}

void UpdateSnakeBody()
{
    // Add the snake's current position to history
    snakePosHistory.Add(new Coord(snakePos.X, snakePos.Y));
    
    // Remove oldest position if tail is longer than it should be
    while (snakePosHistory.Count > tailLength)
    {
        snakePosHistory.RemoveAt(0);
    }
}

void ProcessInputAndDelay()
{
    DateTime time = DateTime.Now;
    bool paused = false;
    
    // This loop handles both the frame delay and input processing
    while ((DateTime.Now - time).TotalMilliseconds < frameDelayMilli || paused)
    {
        if (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (lastMovementDirection != Direction.Right)
                        movementDirection = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    if (lastMovementDirection != Direction.Left)
                        movementDirection = Direction.Right;
                    break;
                case ConsoleKey.UpArrow:
                    if (lastMovementDirection != Direction.Down)
                        movementDirection = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    if (lastMovementDirection != Direction.Up)
                        movementDirection = Direction.Down;
                    break;
                case ConsoleKey.P:
                    paused = !paused;
                    if (paused)
                    {
                        DisplayPauseMessage();
                    }
                    break;
                case ConsoleKey.Escape:
                    gameActive = false;
                    return;
            }
        }
        
        // Small delay to prevent maxing out CPU
        Thread.Sleep(10);
    }
}

// Utility methods
Coord GenerateApplePosition()
{
    // Ensure apple doesn't spawn on snake or walls
    Coord newApplePos;
    do
    {
        newApplePos = new Coord(
            random.Next(1, gridDimensions.X - 1), 
            random.Next(1, gridDimensions.Y - 1)
        );
    } while (snakePos.Equals(newApplePos) || snakePosHistory.Contains(newApplePos));
    
    return newApplePos;
}

void ResetGame()
{
    // Reset all game state variables
    score = 0;
    tailLength = 1;
    snakePos = new Coord(10, 1);
    snakePosHistory.Clear();
    movementDirection = Direction.Down;
    lastMovementDirection = Direction.Down;
    frameDelayMilli = 100;
    applePos = GenerateApplePosition();
}

// Display methods
void DisplayGameInfo()
{
    Console.ForegroundColor = textColor;
    Console.WriteLine($"Score: {score} | High Score: {highScore} | Length: {tailLength}");
    Console.WriteLine("Press ESC to exit, P to pause");
}

void RenderGame()
{
    for (int y = 0; y < gridDimensions.Y; y++)
    {
        for (int x = 0; x < gridDimensions.X; x++)
        {
            Coord currentCoord = new Coord(x, y);
            
            // Determine what to draw at this position
            if (snakePos.Equals(currentCoord))
            {
                Console.ForegroundColor = snakeHeadColor;
                Console.Write("■");
            }
            else if (snakePosHistory.Contains(currentCoord))
            {
                Console.ForegroundColor = snakeBodyColor;
                Console.Write("■");
            }
            else if (applePos.Equals(currentCoord))
            {
                Console.ForegroundColor = appleColor;
                Console.Write("●");
            }
            else if (x == 0 || y == 0 || x == gridDimensions.X - 1 || y == gridDimensions.Y - 1)
            {
                Console.ForegroundColor = wallColor;
                Console.Write("█");
            }
            else
            {
                Console.Write(" ");
            }
        }
        Console.WriteLine();
    }
}

void DisplayWelcomeScreen()
{
    Console.Clear();
    Console.ForegroundColor = textColor;
    Console.WriteLine("\n\n");
    Console.WriteLine("    ███████╗███╗   ██╗ █████╗ ██╗  ██╗███████╗");
    Console.WriteLine("    ██╔════╝████╗  ██║██╔══██╗██║ ██╔╝██╔════╝");
    Console.WriteLine("    ███████╗██╔██╗ ██║███████║█████╔╝ █████╗  ");
    Console.WriteLine("    ╚════██║██║╚██╗██║██╔══██║██╔═██╗ ██╔══╝  ");
    Console.WriteLine("    ███████║██║ ╚████║██║  ██║██║  ██╗███████╗");
    Console.WriteLine("    ╚══════╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝");
    Console.WriteLine("\n\n");
    Console.WriteLine("    Controls:");
    Console.WriteLine("    ↑, ↓, ←, → : Move the snake");
    Console.WriteLine("    P : Pause game");
    Console.WriteLine("    ESC : Exit game");
    Console.WriteLine("    R : Restart (after game over)");
    Console.WriteLine("\n    Press any key to start...");
    
    Console.ReadKey(true);
}

void DisplayGameOverScreen()
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.SetCursorPosition(gridDimensions.X / 2 - 5, gridDimensions.Y / 2 - 1);
    Console.Write("GAME OVER!");
    Console.SetCursorPosition(gridDimensions.X / 2 - 11, gridDimensions.Y / 2);
    Console.Write($"Final Score: {score} | Length: {tailLength}");
    Console.SetCursorPosition(gridDimensions.X / 2 - 13, gridDimensions.Y / 2 + 1);
    Console.Write("Press R to restart or ESC to exit");
}

void DisplayPauseMessage()
{
    Console.ForegroundColor = textColor;
    Console.SetCursorPosition(gridDimensions.X / 2 - 3, gridDimensions.Y / 2);
    Console.Write("PAUSED");
}

