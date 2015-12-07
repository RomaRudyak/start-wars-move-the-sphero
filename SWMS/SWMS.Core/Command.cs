using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SWMS.Core
{
    public class Command<TArgs> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Boolean CanExecute(Object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            TArgs param = SafeCast(parameter);
            return _canExecute(param);

        }

        public void Execute(Object parameter)
        {
            TArgs param = SafeCast(parameter);
            _command(param);
        }

        public Command(Action<TArgs> command)
            : this(command, null)
        {

        }

        public Command(Action<TArgs> command, Func<TArgs, Boolean> canExecute)
        {
            _command = command;
            _canExecute = canExecute;
        }

        private TArgs SafeCast(Object param)
        {
            TArgs arg = default(TArgs);

            try
            {
                arg = (TArgs)param;
            }
            catch (InvalidCastException)
            {
            }

            return arg;
        }

        private Action<TArgs> _command;
        private Func<TArgs, Boolean> _canExecute;
    }

    public class Command : Command<Object>
    {
        public  Command(Action command)
            : this(command, null)
        {

        }

        public Command(Action command, Func<Object, Boolean> canExecute)
            : base(o => command(), canExecute)
        {

        }
    }
}
