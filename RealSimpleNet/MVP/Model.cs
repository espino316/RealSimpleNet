using System.ComponentModel;

/// <summary>
/// Contains MVP classes
/// </summary>
namespace RealSimpleNet.MVP
{
    /// <summary>
    /// Represent a model in the MVP Pattern
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        } // end void NotifyPropertyChanged
    } // end class Model
} // end namespace MVP
