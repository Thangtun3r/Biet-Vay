using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
#nullable enable
public class BietvayMarkerProcessor : ReplacementMarkupHandler
{
    public LineProviderBehaviour? lineProvider;


    private void Start()
    {
        if (lineProvider == null)
        {
            lineProvider = GameObject.FindObjectOfType<LineProviderBehaviour>();
            if (lineProvider == null)
            {
                Debug.LogError("Where is the LineProviderBehaviour man ?");
            }
        }
        lineProvider.RegisterMarkerProcessor("switch", this);
    }

    public override List<LineParser.MarkupDiagnostic> ProcessReplacementMarker(MarkupAttribute marker, StringBuilder childBuilder, List<MarkupAttribute> childAttributes,
        string localeCode)
    {
        if (!marker.TryGetProperty("word", out string? word))
        {
            var diagnostic = new LineParser.MarkupDiagnostic("Missing the switch property, we cannot continue without it.");
            return new List<LineParser.MarkupDiagnostic>() { diagnostic };
        }
        



        return ReplacementMarkupHandler.NoDiagnostics;
    }
}

