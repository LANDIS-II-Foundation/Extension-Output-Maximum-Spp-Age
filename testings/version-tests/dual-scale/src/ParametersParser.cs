//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Species;
using Landis.Util;
using System.Collections.Generic;

namespace Landis.Output.MaxSpeciesAge
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class ParametersParser
        : Landis.TextParser<IParameters>
    {
        public static IDataset SpeciesDataset = null;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return "Max Species Age";
            }
        }

        //---------------------------------------------------------------------

        public ParametersParser()
        {
        }

        //---------------------------------------------------------------------

        protected override IParameters Parse()
        {
            ReadLandisDataVar();

            EditableParameters parameters = new EditableParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<string> mapNames = new InputVar<string>("MapNames");
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> speciesName = new InputVar<string>("Species");
            int lineNumber = LineNumber;
            ReadVar(speciesName);
            if (speciesName.Value.Actual == "all") {
                parameters.SelectedSpecies = SpeciesDataset;
                CheckNoDataAfter("the " + speciesName.Name + " parameter");
            }
            else {
                ISpecies species = GetSpecies(speciesName.Value);
                List<ISpecies> selectedSpecies = new List<ISpecies>();
                selectedSpecies.Add(species);

                Dictionary<string, int> lineNumbers = new Dictionary<string, int>();
                lineNumbers[species.Name] = lineNumber;

                while (! AtEndOfInput) {
                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(speciesName, currentLine);
                    species = GetSpecies(speciesName.Value);
                    if (lineNumbers.TryGetValue(species.Name, out lineNumber))
                        throw new InputValueException(speciesName.Value.String,
                                                      "The species {0} was previously used on line {1}",
                                                      speciesName.Value.String, lineNumber);
                    lineNumbers[species.Name] = LineNumber;

                    selectedSpecies.Add(species);
                    CheckNoDataAfter("the species name", currentLine);
                    GetNextLine();
                }
                parameters.SelectedSpecies = selectedSpecies;
            }

            return parameters.GetComplete();
        }

        //---------------------------------------------------------------------

        protected ISpecies GetSpecies(InputValue<string> name)
        {
            ISpecies species = SpeciesDataset[name.Actual];
            if (species == null)
                throw new InputValueException(name.String,
                                              "{0} is not a species name.",
                                              name.String);
            return species;
        }
    }
}
