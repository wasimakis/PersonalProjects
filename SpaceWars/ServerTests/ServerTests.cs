using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Setting;
using WorldObjects;

namespace ServerTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void Iterable()
        {
            World world = new World();
            world.Ships.Add(2, new Ship());
            foreach (Ship s in world.Ships.Values) {
              
            }
            Assert.AreEqual("HJELASLKDJA", world.Ships[2].Command);

        }
    }
}
