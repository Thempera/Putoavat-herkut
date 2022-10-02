using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
// Yhteenlasketut työtunnit:
// Tuuli: 28h
// Henna: 29h

/// @author Tuuli Mantila ja Henna Sillanpää
/// @version 2019
/// <summary>
/// 
/// </summary>
public class Putoavat_herkut : PhysicsGame
{
    private PhysicsObject olio;


    private Timer ajastin; //alustetaan beginissä
    private IntMeter pisteLaskuri;
    private MultiSelectWindow alkuValikko;

    private readonly Image taustaKuva = LoadImage("tausta");
    private readonly SoundEffect pieruAani = LoadSoundEffect("PIERU");
    private readonly SoundEffect heiAani = LoadSoundEffect("HEI");
    //private readonly SoundEffect nielaisuAani = LoadSoundEffect("NIELAISU");


    // Vektorit oikea ja vasen, joita käytetään olion nopeuden määrittelyssä näppäimiä käyttäessä.
    private readonly Vector vasen = new Vector(-550, 0);
    private readonly Vector oikea = new Vector(550, 0);

    // Kuvat herkuille
    private readonly Image pommi = LoadImage("POMMI");
    private readonly Image karkki = LoadImage("KARKKI");
    private readonly Image porkkana = LoadImage("PORKKANA");
    private readonly Image likasukka = LoadImage("LIKASUKKA");
    private readonly Image kakku = LoadImage("KUPPIKAKKU");
    private readonly Image pallo = LoadImage("JALKAPALLO");


    /// <summary>
    /// Sisältää olion luonnin, taustamusiikin ja ajastimen.
    /// </summary>
    public override void Begin()
    {

        Valikko();
        IsFullScreen = true;
        //SetWindowSize(1600, 900, true);
        AsetaOhjaimet();

        Level.Background.CreateGradient(Color.Lime, Color.Blue);
        Level.CreateBorders(1.0, false, Color.Black);
        Level.Background.Image = taustaKuva;

        
        olio = new PhysicsObject(150.0, 150.0);
        olio.IgnoresPhysicsLogics = true;
        olio.CanRotate = false;
        olio.Restitution = (0.0);
        olio.Y = -270.0;
        olio.Image = LoadImage("HERKKUPELIUKKO");


        MediaPlayer.Play("TAUSTAMUSIIKKI");
        MediaPlayer.IsRepeating = true;

        ajastin = new Timer();
        ajastin.Interval = 1.5;
        ajastin.Timeout += LuoHerkkuja;

        LuoPistelaskuri();

        for (int eka = 50; eka < 1000; eka = eka*2)
        {
            pisteLaskuri.AddTrigger(eka, TriggerDirection.Up, MuutaAjastinta);
        }

        Gravity = new Vector(0, -320);

        Camera.ZoomToLevel();        
    }


    /// <summary>
    /// Aloittaa uuden pelin.
    /// </summary>
    public void AloitaPeli()
    {
        Add(olio);
        pisteLaskuri.Reset();
        ajastin.Reset();
        ajastin.Interval = 1.5;
        ajastin.Start();      
    }


    /// <summary>
    /// Luo aloitusvalikon.
    /// </summary>
    public void Valikko()
    {
        alkuValikko = new MultiSelectWindow("Valikko", "Aloita peli", "Lopeta");
        alkuValikko.Color = Color.LightGreen;
        Mouse.IsCursorVisible = true;
        alkuValikko.AddItemHandler(0, AloitaPeli);
        alkuValikko.AddItemHandler(1, Exit);
        alkuValikko.DefaultCancel = 3;
        Add(alkuValikko);
    }


    ///<summary>
    ///Luodaan uusia herkkuja toisten pudotessa
    ///</summary>
    public void LuoHerkkuja()
    {
       PhysicsObject herkku = new PhysicsObject(2 * 40.0, 2 * 40.0);
        herkku.Color = Color.HotPink;
        herkku.IgnoresCollisionResponse = true;
        herkku.Y = 500.0;
        herkku.X = RandomGen.NextDouble(-470, 470);
        herkku.AngularVelocity = RandomGen.NextDouble(3.0, 7.0);
        int[] pisteet = { 0 , 5 , 10, -7, 3, -10 };
        Image[] herkut = {pommi, karkki, porkkana, likasukka, kakku, pallo};
        

        int i = RandomGen.NextInt(herkut.Length);
        herkku.Image = herkut[i];
        herkku.Tag = pisteet[i];
        Add(herkku);

        AddCollisionHandler(olio, herkku, OlioSyoHerkut);
}


    ///<summary>
    ///Olio syö herkut
    ///</summary>
    public void OlioSyoHerkut(PhysicsObject olio, PhysicsObject herkku)
    {
        int herkkuPiste = (int)(herkku.Tag);
        if (herkkuPiste < 0) heiAani.Play();
        //if (herkkuPiste > 0) nielaisuAani.Play();
        Remove(herkku);

        pisteLaskuri.Value += herkkuPiste;

        if (herkkuPiste == 0)

        {
            Remove(olio);
            ajastin.Stop();
            pieruAani.Play();
            PhysicsObject lima = PhysicsObject.CreateStaticObject(800.0, 800.0);
            lima.Image = LoadImage("LIMA");
            lima.LifetimeLeft = TimeSpan.FromSeconds(3.0); ;
            Add(lima);
            Timer.SingleShot(3.0, Valikko);
        }
    }
    

    /// <summary>
    /// Aliohjelma, jossa määritetään pelissä käytettävät näppäimet ja niiden toiminnot.
    /// </summary>
    void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeusV, "Liikuta", vasen);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeusO, "Liikuta", oikea);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, Vector.Zero);

        Keyboard.Listen(Key.P, ButtonState.Pressed, Pause, "Pysäyttää pelin");

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");


    }


    /// <summary>
    /// Laittaa pelin tauolle eli pauselle.
    /// </summary>
    void Pause()
    {
        if (IsPaused == false) IsPaused = true;
        else IsPaused = false;
    }


    /// <summary>
    /// Aliohjelma, jota hyödynnetään pelinäppäinten asettamisessa AsetaOhjaimet-aliohjelmassa. 
    /// </summary>
    /// <param name="nopeus"></param>
    void AsetaNopeus(Vector nopeus)
    {
        olio.Velocity = nopeus;
    }


    /// <summary>
    /// Aliohjelma, jota hyödynnetään pelinäppäinten asettamisessa AsetaOhjaimet-aliohjelmassa . Vasen suunta.
    /// </summary>
    /// <param name="nopeus"></param>
    void AsetaNopeusV(Vector nopeus)
    {
        olio.Velocity = nopeus;
        olio.Image = LoadImage("HERKKUPELIUKKOV");
    }


    /// <summary>
    /// Aliohjelma, jota hyödynnetään pelinäppäinten asettamisessa AsetaOhjaimet-aliohjelmassa. Oikea suunta.
    /// </summary>
    /// <param name="nopeus"></param>
    void AsetaNopeusO(Vector nopeus)
    {
        olio.Velocity = nopeus;
        olio.Image = LoadImage("HERKKUPELIUKKO");
    }


    /// <summary>
    /// Luodaan pistelaskuri
    /// </summary>
    void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Right - 100;
        pisteNaytto.Y = 280;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Pisteet";
        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Muuttaa ajastinta.
    /// </summary>
    void MuutaAjastinta()
    {
        double muutos = -0.2;
        if (ajastin.Interval + muutos < 0) return;
        ajastin.Interval += muutos;
    }

}

