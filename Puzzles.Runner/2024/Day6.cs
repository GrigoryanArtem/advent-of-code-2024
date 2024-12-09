using System.Runtime.Serialization.Formatters;

namespace Puzzles.Runner._2024;

[Puzzle("Guard Gallivant", 6, 2024)]
public class Day6(ILinesInputReader input) : IPuzzleSolver
{
    #region Constants

    private const int START_DIRECTION = 0;
    private const int HAS_LOOP = -1;
    private const int NO_JUMP = -1;

    private const byte EMPTY = 0;
    private const byte OBSTRUCTION = 1;
    private const byte BORDER = 2;

    private const char OBSTRUCTION_SYMBOL = '#';
    private const char GUARD_SYMBOL = '^';

    private const int NUMBER_OF_TASKS = 1;

    #endregion

    #region Members

    private int[] _directions = [];
    private byte[] _map = [];

    private int _location = 0;
    private int _sx = 0;

    private byte[][] _buffers = [];
    private int[,] _jumps = new int[0, 0];

    #endregion

    public void Init()
    {
        _sx = input.Lines.First().Length + 2;
        var sy = input.Lines.Length + 2;

        _map = new byte[_sx * sy];

        for (int x = 0; x < _sx; x++)
            _map[Mat2Vec(x, 0)] = _map[Mat2Vec(x, sy - 1)] = BORDER;

        for (int y = 0; y < sy; y++)
            _map[Mat2Vec(0, y)] = _map[Mat2Vec(_sx - 1, y)] = BORDER;

        for (int y = 0; y < input.Lines.Length; y++)
        {
            for (int x = 0; x < input.Lines[y].Length; x++)
            {
                if (input.Lines[y][x] == OBSTRUCTION_SYMBOL)
                    _map[Mat2Vec(x + 1, y + 1)] = OBSTRUCTION;

                if (input.Lines[y][x] == GUARD_SYMBOL)
                    _location = Mat2Vec(x + 1, y + 1);
            }
        }

        _directions = [-sy, 1, sy, -1];

        _buffers = new byte[NUMBER_OF_TASKS][];
        for (int i = 0; i < NUMBER_OF_TASKS; i++)
            _buffers[i] = new byte[_map.Length];

        CalculateJumps();
    }
    
    private void CalculateJumps()
    {
        _jumps = new int[_map.Length, _directions.Length];
        ResetJumps();

        for (int i = 0; i < _map.Length; i++)
        {
            if (_map[i] == OBSTRUCTION)
            {
                for (int d = 0; d < _directions.Length; d++)
                {
                    var location = i + _directions[d];
                    if (_map[location] == EMPTY)
                        CalculateJumps(_jumps, location, -1);
                }
            }
        }

        CalculateJumps(_jumps, _location, 0, -1);
    }

    private void CalculateJumps(int[,] buffer, int obstruction)
    {
        ResetJumps();

        for (int i = 0; i < _map.Length; i++)
        {
            if (_map[i] == OBSTRUCTION || i == obstruction)
            {
                for (int d = 0; d < _directions.Length; d++)
                {
                    var location = i + _directions[d];
                    if (_map[location] == EMPTY)
                        CalculateJumps(buffer, location, obstruction);
                }
            }
        }

        CalculateJumps(buffer, _location, obstruction);
    }

    private void ResetJumps()
    {
        for (int i = 0; i < _map.Length; i++)
            for (int k = 0; k < _directions.Length; k++)
                _jumps[i, k] = NO_JUMP;
    }

    private void RecalculateJumpsRow(int [,] buffer, int row, int obstacleLocation) 
    {
        for (int i = _sx * row; i < _sx * (row + 1); i++)
        {
            var update = _directions.Any(d => _map[i + d] == OBSTRUCTION || i + d == obstacleLocation);
            if (update) CalculateJumps(buffer, i, obstacleLocation);
        }

        CalculateJumps(buffer, _location, 0, obstacleLocation);        
    }

    private void RecalculateJumpsColumn(int[,] buffer, int column, int obstacleLocation)
    {        
        for (int i = _sx + _sx + column; i < _map.Length - _sx; i += _sx)
        {
            var update = _directions.Any(d => _map[i + d] == OBSTRUCTION || i + d == obstacleLocation);
            if (update) CalculateJumps(buffer, i, obstacleLocation);
        }

        CalculateJumps(buffer, _location, 0, obstacleLocation);
    }

    private void CalculateJumps(int[,] buffer, int location, int obstacleLocation)
    {
        for(int drx = 0; drx < _directions.Length; drx++)        
            CalculateJumps(buffer, location, drx, obstacleLocation);        
    }

    private void CalculateJumps(int[,] buffer, int location, int drx, int obstacleLocation)
    {        
        for (int path = 0, jump = location; ;path++)
        {
            var next = jump + _directions[drx];
            if (_map[next] == OBSTRUCTION || next == obstacleLocation)
            {                
                buffer[location, drx] = path > 0 ? jump : location;
                break;
            }

            if (_map[next] == BORDER)
            {
                buffer[location, drx] = next;
                break;
            }

            jump = next;
        }
    }

    public string SolvePart1()
        => FindPath(_location, START_DIRECTION, _buffers[0]).ToString();

    public string SolvePart2()
    {
        //var chunkSize = (int)Math.Ceiling((double)_map.Length / NUMBER_OF_TASKS);
        //var tasks = Enumerable.Range(0, NUMBER_OF_TASKS)
        //    .Select(i => BrutForceAsync(i * chunkSize, chunkSize, _buffers[i]))
        //    .ToArray();

        //Task.WaitAll(tasks);

        //return tasks.Sum(t => t.Result).ToString();
        return BruteForce(_sx, _map.Length - 2 * _sx, _buffers[0]).ToString();
    }

    #region Private methods

    private Task<int> BrutForceAsync(int start, int count, byte[] buffer)
        => Task.Run(() => BruteForce(start, count, buffer));

    private int BruteForce(int start, int count, byte[] buffer)
    {
        var jumpBuffer = new int[_map.Length, _directions.Length];
        

        var end = Math.Min(start + count, _map.Length);
        int sum = 0;

        for (int i = start; i < end; i++)
        {
            Array.Copy(_jumps, jumpBuffer, _jumps.Length);

            var (row, column) = Vec2Mat(i);
        
            RecalculateJumpsRow(jumpBuffer, row , i);    
            RecalculateJumpsColumn(jumpBuffer, column, i);

            if (_map[i] == EMPTY && FindLoop(_location, START_DIRECTION, buffer, jumpBuffer))
                sum++;
        }

        return sum;
    }

    private bool FindLoop(int location, int direction, byte[] buffer, int[,] jumpBuffer)
    {
        Array.Clear(buffer);

        while (_map[location] != BORDER)
        {
            var df = Dir2Flg(direction);
            if ((buffer[location] & df) == df)
                return true;

            buffer[location] |= df;
            location = jumpBuffer[location, direction];
            direction = (direction + 1) % _directions.Length;
        }

        return false;
    }

    private int FindPath(int location, int direction, byte[] buffer)
    {
        Array.Clear(buffer);

        while (_map[location] != BORDER)
        {
            var de = Dir2Flg(direction);
            if ((buffer[location] & de) == de)
                return HAS_LOOP;

            buffer[location] |= de;

            var next = location + _directions[direction];
            if (_map[next] == OBSTRUCTION)
            {
                direction = (direction + 1) % _directions.Length;
            }
            else
            {
                location = next;
            }
        }

        return buffer.Count(d => d > 0);
    }

    private static byte Dir2Flg(int direction)
        => (byte)(1 << direction);

    private int Mat2Vec(int x, int y)
        => y * _sx + x;
    private (int x, int y) Vec2Mat(int location)
        => (location / _sx, location % _sx);

    #endregion
}
