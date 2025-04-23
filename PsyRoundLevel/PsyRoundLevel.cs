// ----------------------------------------------------------------------------
// Copyright (c) 2025 https://github.com/qtx-project
// Licensed under the **[GPL-3.0 License](./license.txt)**.
// See LICENSE file in the project root for full license information.
// ----------------------------------------------------------------------------

namespace PsyRoundLevel;

using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;

/// <summary>
/// <c>PsyLevel</c> is a custom indicator designed to highlight psychological round price levels
/// on a financial chart. These levels, such as 50, 100, 1000, and 10000, often act as 
/// natural support or resistance zones due to human trading behavior.
/// 
/// This indicator visualizes multiple levels with different levels of transparency
/// to indicate their significance. 
/// </summary>
public class PsyLevel : Indicator
{ 
    /// <summary>
    /// Pen used to draw round level lines with varying importance or strength.
    /// Each pen corresponds to a different magnitude of psychological level,
    /// differentiated by transparency (alpha channel).
    /// </summary>
    private Pen _penColor10000;       // Most significant level (e.g., 10000, 20000)
    private Pen _penColor1000;        // Major level (e.g., 1000, 2000)
    private Pen _penColor100;         // Medium significance (e.g., 100, 200)
    private Pen _penColor50;          // Minor level (e.g., 50, 150)
    private Pen _penColorBeforeEnd;   // Near-end marker or visual cue
    private Pen _penColorEnd;         // Terminal/last level line, very subtle
    
    #region InputParameter

    /// <summary>
    /// Defines the color used to draw round level lines on the chart.
    /// Can be customized by the user to match their preferred chart theme.
    /// </summary>
    [InputParameter("Color Line Round Level", 100)]
    public Color ColorRoundLevel;

    /// <summary>
    /// Sets the base unit used to calculate round (psychological) price levels.
    /// For example, a value of 50 will draw levels at 50, 100, 150, etc.
    /// </summary>
    [InputParameter("Factor Round Level", 101)]
    public int FactorRoundLevel;

    /// <summary>
    /// Enables developer-level debug messages in the platform’s log output.
    /// Useful for troubleshooting or development purposes.
    /// </summary>
    [InputParameter("Enable Dev Debug", 1000)]
    public bool HasDebugEnabled;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <c>PsyLevel</c> indicator with default settings.
    /// Sets the name, description, visual behavior, and initializes round level rendering colors.
    /// </summary>
    public PsyLevel()
    {
        Name = "PsyRoundLevel";
        Description = "All Right Reserved";
        SeparateWindow = false;
        HasDebugEnabled = false;
        FactorRoundLevel = 50;
        ColorRoundLevel = Color.FromArgb(255, 110, 222, 250);
        UpdateColor(); // instanciate default Pen object
    }
    
    /// <summary>
    /// Called once during the initialization phase of the indicator.
    /// This method sets up the initial state and logs useful debug information about the instance.
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();
        Debug($"Instanciate {Name} on {Symbol.Name}");
    }
 
    /// <summary>
    /// Called automatically when the indicator's settings are modified
    /// This override ensures that any visual-related changes, such as colors, are immediately applied.
    /// </summary>
    protected override void OnSettingsUpdated()
    {
        base.OnSettingsUpdated();
        UpdateColor();
    }

    /// <summary>
    /// Updates the pens used to draw round levels with varying opacities based on their importance.
    /// Each level (e.g., 10000, 1000, 100) is assigned a distinct alpha value to visually differentiate them.
    /// </summary>
    private void UpdateColor()
    {
        _penColor10000 = new Pen(Color.FromArgb(255, ColorRoundLevel), 1);
        _penColor1000 = new Pen(Color.FromArgb(233, ColorRoundLevel), 1);
        _penColor100 = new Pen(Color.FromArgb(144, ColorRoundLevel), 1);
        _penColor50 = new Pen(Color.FromArgb(89, ColorRoundLevel), 1);
        _penColorBeforeEnd = new Pen(Color.FromArgb(55, ColorRoundLevel), 1);
        _penColorEnd = new Pen(Color.FromArgb(34, ColorRoundLevel), 1);
    }
    
    /// <summary>
    /// Called by the charting engine to render custom graphics on the chart.
    /// </summary>
    /// <param name="args">
    /// The event arguments containing the drawing context and other chart-related information.
    /// </param>
    public override void OnPaintChart(PaintChartEventArgs args)
    {
        base.OnPaintChart(args);
        OnPaintRoundLevel(args);
    }

    /// <summary>
    /// Handles the custom rendering of round levels on the chart.
    /// Round levels are typically key psychological price points (e.g., 21500, 21000, ....)
    /// that traders use to identify support/resistance zones.
    /// </summary>
    /// <param name="args">
    /// The chart painting event arguments, providing access to the drawing surface and price/time context.
    /// </param>
    private void OnPaintRoundLevel(PaintChartEventArgs args)
    {
        var coordinateConverter = CurrentChart.Windows[args.WindowIndex].CoordinatesConverter;

        var floorMinP = Math.Floor(coordinateConverter.GetPrice(args.Rectangle.Height) / FactorRoundLevel) *
                        FactorRoundLevel;

        var totalLineByFactor =
            (int)((Math.Floor(coordinateConverter.GetPrice(0) / FactorRoundLevel) * FactorRoundLevel) - floorMinP) /
            FactorRoundLevel;

        var margin = args.Rectangle.Height / 8;

        // debug
        if (HasDebugEnabled)
        {
            var to = Math.Floor(coordinateConverter.GetPrice(0) / FactorRoundLevel) * FactorRoundLevel;
            var from = Math.Floor(coordinateConverter.GetPrice(args.Rectangle.Height) / FactorRoundLevel) *
                       FactorRoundLevel;
            Debug($"Update Lines Round Level: from {from} to {to} <- {totalLineByFactor}");
        }

        // generate lines
        for (var i = 0; i < totalLineByFactor; i++)
        {
            var currentStep = floorMinP + (i * FactorRoundLevel);
            var y = (int)coordinateConverter.GetChartY(currentStep);
            if (y >= 0 && y < this.CurrentChart.Windows[args.WindowIndex].ClientRectangle.Height)
            {
                args.Graphics.DrawLine(
                    (currentStep % 10000 == 0)
                        ? _penColor10000
                        : (currentStep % 1000 == 0)
                        ? _penColor1000
                        : (y <= margin)
                        ? _penColorEnd
                        : (y >= args.Rectangle.Height - margin)
                        ? _penColorEnd
                        : (y <= margin * 2)
                        ? _penColorBeforeEnd
                        : (y >= args.Rectangle.Height - (margin * 2))
                        ? _penColorBeforeEnd
                        : (currentStep % 100 == 0)
                        ? _penColor100
                        : _penColor50,
                    0,
                    y,
                    args.Rectangle.Width,
                    y
                );
            }
        }
    }
 
    /// <summary>
    /// Logs a debug message to the trading system’s logger if debugging is enabled.
    /// This is useful for monitoring indicator behavior during development or live testing.
    /// </summary>
    /// <param name="message">
    /// The message to log. Should describe the context or state relevant to the indicator logic.
    /// </param>
    private void Debug(string message)
    {
        if (HasDebugEnabled)
        {
            Core.Loggers.Log(message, LoggingLevel.Trading, $"{Name} {Symbol.Name} : Indicator");
        }
    }
    
    /// <summary>
    /// Gets the short identifier used to represent this indicator in user interfaces or logs.
    /// </summary>
    public override string ShortName => "LvL";

    /// <summary>
    /// Gets the URL linking to the official help page or user documentation for this indicator.
    /// - This link usually explains the purpose, configuration options, and usage examples.
    /// </summary>
    public override string HelpLink => $"https://github.com/qtx-project/psy-round-level";

    /// <summary>
    /// Gets the URL pointing to the public source code repository of this indicator.
    /// - This link allows developers to inspect, contribute to, or fork the underlying implementation.
    /// </summary>
    public override string SourceCodeLink => $"https://github.com/qtx-project/psy-round-level";
    
}
