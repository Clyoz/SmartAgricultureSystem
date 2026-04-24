using System;
using System.Windows.Input;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// MVVM模式下的命令实现
    /// 将方法绑定到WPF控件的Command属性
    /// </summary>
    public class RelayCommand : ICommand
    {
        // 命令执行方法
        private readonly Action<object> mExecute;

        // 命令是否可执行的判断方法
        private readonly Func<object, bool> mCanExecute;

        /// <summary>
        /// 可执行状态变更事件
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">是否可执行判断（可选）</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            mExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            mCanExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可执行
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return mCanExecute == null || mCanExecute(parameter);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public void Execute(object parameter)
        {
            mExecute(parameter);
        }
    }
}