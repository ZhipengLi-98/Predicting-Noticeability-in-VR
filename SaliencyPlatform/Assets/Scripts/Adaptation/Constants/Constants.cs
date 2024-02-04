using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
    public const int MAX_RATING = 7;
    public const int MIN_RATING = 1;

    public const double MIN_ELEMENT_SCALE = 1;
    public const double MAX_ELEMENT_SCALE = 2;

    public enum Dimensionality { TwoDimensional, ThreeDimensional };

    public enum ElementSet { Productivity, Leisure, UserStudy };
    //public static readonly string[] productivityElements = new string[]{ "paper-1", "paper-2", "web-1", "web-2", "image-1", "image-2", "image-3", "image-4", "GraphVisualization", "ParallelCoordinates", "ScatterPlot", "HMDModel" };
    public static readonly string[] productivityElements = new string[] { "paper-1", "web-2", "ScatterPlot", "calendar", "WeatherWidget", "Time", "Slack", "MusicApplication", "WordProcessor", "TODOList" };
    //public static readonly string[] leisureElements = new string[]{ "news-1", "news-3", "news-5", "icon-1", "icon-2", "icon-3", "icon-4", "Time", "WeatherWidget", "messenger", "calendar", "ShoppingApplication"};
    public static readonly string[] leisureElements = new string[] { "messenger", "WeatherWidget", "Time", "news-1", "photoSharing", "videoStreaming", "calendar", "health", "TODOList", "IconMenu"};
    //public static readonly string[] studyElements = new string[]{ "paper-1", "web-2", "ScatterPlot", "calendar", "WeatherWidget", "Time", "Slack", "MusicApplication", "WordProcessor", "TODOList" };

    public static readonly List<string> planarElements = new List<string> { "paper-1", "paper-2", "web-1", "web-2", "image-1", "image-2", "image-3", "image-4", "news-1", "news-3", "news-5", "messenger", "calendar" };

    public enum Environments { Empty, Office, OfficeAddedObjects, Bedroom, BedroomAddedObjects, Coffeeshop, CoffeeshopAddedObjects, Livingroom, LivingroomAddedObjects, Bedroom2, MeetingRoomA, MeetingRoomB };

    public static readonly Vector2[] manualLayoutInitPlacement = new Vector2[]{
        new Vector2(1.0f, 1.0f),
        new Vector2(-1.0f, 1.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(-0.5f, 0.0f),
        new Vector2(0.5f, 0.0f),
        new Vector2(-1.5f, 0.0f),
        new Vector2(1.5f, 0.0f),
        new Vector2(1.0f, -1.0f),
        new Vector2(-1.0f, -1.0f),
        new Vector2(0.0f, -1.0f),
    };

    public enum SpatialDimensions { x, xAbs, y, yAbs, z, zAbs, dist, fromForward };
    public static readonly List<SpatialDimensions> allSpatialDimensions = new List<SpatialDimensions> {
        SpatialDimensions.x,
        SpatialDimensions.xAbs,
        SpatialDimensions.y,
        SpatialDimensions.yAbs,
        SpatialDimensions.z,
        SpatialDimensions.zAbs,
        SpatialDimensions.dist,
        SpatialDimensions.fromForward
    };
}
