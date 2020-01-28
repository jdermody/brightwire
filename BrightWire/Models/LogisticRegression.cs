using BrightData;

namespace BrightML.Models
{
    public class LogisticRegression : IModel
    {
        public Vector<float> Theta { get; }

        public LogisticRegression(Vector<float> theta)
        {
            Theta = theta;
        }

        public void Dispose()
        {
            Theta.Dispose();
        }
    }
}
