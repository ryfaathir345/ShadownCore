using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace WinTweakStudio.Services
{
    public interface ISoundEffectService
    {
        void PlayBoostOn();
        void PlayBoostOff();
        void PlayProfileSwitch();
    }

    public class SoundEffectService : ISoundEffectService
    {
        public void PlayBoostOn()
        {
            Task.Run(() =>
            {
                try
                {
                    // Synthesize futuristic high-pitch beep boost sound using Windows SystemSounds / Beep
                    Console.Beep(800, 100);
                    Console.Beep(1200, 120);
                    Console.Beep(1800, 180);
                }
                catch
                {
                    SystemSounds.Asterisk.Play();
                }
            });
        }

        public void PlayBoostOff()
        {
            Task.Run(() =>
            {
                try
                {
                    // Synthesize power down sound
                    Console.Beep(1400, 100);
                    Console.Beep(900, 120);
                    Console.Beep(500, 150);
                }
                catch
                {
                    SystemSounds.Hand.Play();
                }
            });
        }

        public void PlayProfileSwitch()
        {
            Task.Run(() =>
            {
                try
                {
                    Console.Beep(1000, 80);
                    Console.Beep(1500, 100);
                }
                catch
                {
                    SystemSounds.Exclamation.Play();
                }
            });
        }
    }
}
