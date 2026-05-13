using Math_Dice_Fixed_V2.Commands;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace Math_Dice_Fixed_V2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Random _rand = new();

        private int _target;
        private int[] _dice = Array.Empty<int>();
        private int _turn = 1;
        private bool _gameOver;

        private string _targetText = "";
        private string _diceText = "";
        private string _statusText = "";
        private string _logText = "";
        private string _expression = "";
        private string _validationText = "";

        private Brush _statusBrush = Brushes.LimeGreen;
        private Brush _validationBrush = Brushes.Black;

        public MainViewModel()
        {
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
            RestartCommand = new RelayCommand(StartGame);

            StartGame();
        }

        public string TargetText
        {
            get => _targetText;
            set => SetProperty(ref _targetText, value);
        }

        public string DiceText
        {
            get => _diceText;
            set => SetProperty(ref _diceText, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public Brush StatusBrush
        {
            get => _statusBrush;
            set => SetProperty(ref _statusBrush, value);
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        public string Expression
        {
            get => _expression;
            set
            {
                if (SetProperty(ref _expression, value))
                {
                    ValidateExpression();
                    RaiseCanExecuteChanged();
                }
            }
        }

        public string ValidationText
        {
            get => _validationText;
            set => SetProperty(ref _validationText, value);
        }

        public Brush ValidationBrush
        {
            get => _validationBrush;
            set => SetProperty(ref _validationBrush, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand RestartCommand { get; }

        private void StartGame()
        {
            _target = _rand.Next(1, 13) * _rand.Next(1, 13);
            _dice = new[]
            {
                _rand.Next(1, 7),
                _rand.Next(1, 7),
                _rand.Next(1, 7),
                _rand.Next(1, 7)
            };

            _turn = 1;
            _gameOver = false;

            TargetText = $"🎯 목표 숫자: {_target}";
            DiceText = $"🎲 주사위: {string.Join(", ", _dice)}";
            StatusText = "진행 중";
            StatusBrush = Brushes.LimeGreen;
            LogText = "=== Math Dice 게임 시작! ===\n";
            Expression = "";
            ValidationText = "";
            ValidationBrush = Brushes.Black;

            RaiseCanExecuteChanged();
        }

        private void ValidateExpression()
        {
            if (_gameOver)
                return;

            string expr = Expression.Trim();

            if (string.IsNullOrEmpty(expr))
            {
                ValidationText = "";
                return;
            }

            if (!Regex.IsMatch(expr, @"^[0-9+\-*/()]*$"))
            {
                ValidationText = "❌ 숫자와 연산자(+,-,*,/)만 사용 가능합니다!";
                ValidationBrush = Brushes.Red;
                return;
            }

            string[] numbersInExpr = Regex.Matches(expr, @"\d+")
                                          .Cast<Match>()
                                          .Select(m => m.Value)
                                          .ToArray();

            bool valid = numbersInExpr.All(n => CanMakeNumberFromDice(n, _dice));

            if (valid)
            {
                ValidationText = "✅ 사용 가능한 주사위 숫자만 사용 중입니다.";
                ValidationBrush = Brushes.LimeGreen;
            }
            else
            {
                ValidationText = "❌ 주사위 숫자를 잘못 사용했습니다!";
                ValidationBrush = Brushes.Red;
            }
        }

        private bool CanSubmit()
        {
            return !_gameOver;
        }

        private void Submit()
        {
            if (_gameOver)
                return;

            string expr = Expression.Trim();

            if (string.IsNullOrEmpty(expr))
            {
                AppendLog("⚠️ 수식을 입력하세요.");
                return;
            }

            AppendLog($"\n--- {_turn} 턴 ---");
            AppendLog($"입력한 수식: {expr}");

            if (!Regex.IsMatch(expr, @"^[0-9+\-*/()]*$"))
            {
                AppendLog("❌ 숫자와 연산자(+,-,*,/)만 사용 가능합니다!");
                return;
            }

            string[] numbersInExpr = Regex.Matches(expr, @"\d+")
                                          .Cast<Match>()
                                          .Select(m => m.Value)
                                          .ToArray();

            if (!numbersInExpr.All(n => CanMakeNumberFromDice(n, _dice)))
            {
                AppendLog("❌ 주사위 숫자를 잘못 사용했습니다!");
                EndGame(false);
                return;
            }

            try
            {
                var dt = new DataTable();
                int result = Convert.ToInt32(dt.Compute(expr, ""));

                AppendLog($"계산 결과: {result}");

                if (result == _target)
                {
                    AppendLog("✅ 정답! 승리했습니다!");
                    EndGame(true);
                }
                else
                {
                    AppendLog($"❌ 오답! 목표({_target})와 다릅니다.");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"⚠️ 잘못된 수식: {ex.Message}");
            }

            _turn++;
        }

        private bool CanMakeNumberFromDice(string numberStr, int[] diceArray)
        {
            int[] diceCopy = (int[])diceArray.Clone();

            foreach (char c in numberStr)
            {
                int digit = c - '0';
                int index = Array.IndexOf(diceCopy, digit);

                if (index == -1)
                    return false;

                diceCopy[index] = -1;
            }

            return true;
        }

        private void AppendLog(string message)
        {
            LogText += message + "\n";
        }

        private void EndGame(bool win)
        {
            _gameOver = true;

            AppendLog("\n=== 게임 종료 ===");

            if (win)
            {
                AppendLog("🎉 당신이 이겼습니다!");
                StatusText = "승리";
                StatusBrush = Brushes.LimeGreen;
            }
            else
            {
                AppendLog("💀 패배했습니다.");
                StatusText = "패배";
                StatusBrush = Brushes.Red;
            }

            RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            if (SubmitCommand is RelayCommand submitCommand)
                submitCommand.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
