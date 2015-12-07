using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Core
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]String propName="")
        {
            var observers = PropertyChanged;
            if (observers != null)
            {
                observers(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
