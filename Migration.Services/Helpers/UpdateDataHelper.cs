﻿using Migration.Repository.Extensions;
using Migration.Repository.Helpers;
using Migration.Repository.Models;
using Newtonsoft.Json.Linq;

namespace Migration.Services.Helpers
{
    public static class UpdateDataHelper
    {
        public static JObject UpdateObject(string sourceObj, List<DataFieldsMapping> mappingMergeFields, ref bool hasChange)
        {
            var objectToBeUpdated = JObject.Parse(sourceObj);

            foreach (var mappingMergeField in mappingMergeFields.Where(w => w.MappingType != MappingType.TableJoin))
            {
                if (mappingMergeField.MappingType == MappingType.MergeFieldWithCondition ||
                    mappingMergeField.MappingType == MappingType.UpdateValueWithCondition)
                {
                    var meetCriteria =
                        objectToBeUpdated.MeetCriteriaSearch(mappingMergeField.Conditions.Select(s => s));

                    if (meetCriteria)
                    {
                        var fieldArr = mappingMergeField.SourceField.Split(".").ToList();
                        objectToBeUpdated =
                            JObjectHelper.UpdateObject(objectToBeUpdated, fieldArr, mappingMergeField.ValueField);
                        hasChange = true;
                    }
                }
                else if (mappingMergeField.MappingType == MappingType.UpdateValue)
                {
                    var newValue = MapFieldTypes.GetType(mappingMergeField);

                    var fieldArr = mappingMergeField.DestinationField.Split(".").ToList();

                    objectToBeUpdated = JObjectHelper.UpdateObject(objectToBeUpdated, fieldArr, newValue);
                    hasChange = true;
                }
                else
                {
                    var valueFromSource = JObjectHelper.GetValueFromObject(objectToBeUpdated, mappingMergeField.SourceField.Split(".").ToList());

                    var fieldsFromDestinationArr = mappingMergeField.DestinationField.Split(".").ToList();

                    objectToBeUpdated =
                        JObjectHelper.UpdateObject(objectToBeUpdated, fieldsFromDestinationArr, valueFromSource);
                    hasChange = true;
                }
            }

            return objectToBeUpdated;
        }

        public static JObject UpdateObject(string destinationData, List<DataFieldsMapping> mappingMergeFields, JObject sourceObj, ref bool hasChange)
        {
            var objectToBeUpdated = JObject.Parse(destinationData);

            foreach (var mappingMergeField in mappingMergeFields.Where(w => w.MappingType != MappingType.TableJoin))
            {
                if (mappingMergeField.MappingType == MappingType.MergeFieldWithCondition ||
                    mappingMergeField.MappingType == MappingType.UpdateValueWithCondition)
                {
                    var sourceConditions = mappingMergeField.Conditions
                        .Where(w => w.ConditionDirection == MappingDirectionType.Source).Select(s => s);

                    var destinationConditions = mappingMergeField.Conditions
                        .Where(w => w.ConditionDirection == MappingDirectionType.Destination).Select(s => s);


                    var meetCriteria =
                        sourceConditions.Any(a => a.Type == SearchConditionType.Or) || destinationConditions.Any(a => a.Type == SearchConditionType.Or)
                            ? sourceObj.MeetCriteriaSearch(sourceConditions) || objectToBeUpdated.MeetCriteriaSearch(destinationConditions)
                            : sourceObj.MeetCriteriaSearch(sourceConditions) && objectToBeUpdated.MeetCriteriaSearch(destinationConditions);

                    if (meetCriteria)
                    {
                        var fieldsArr = mappingMergeField.DestinationField.Split(".").ToList();

                        var value = mappingMergeField.MappingType == MappingType.UpdateValueWithCondition
                            ? MapFieldTypes.GetType(mappingMergeField)
                            : JObjectHelper.GetValueFromObject(sourceObj, mappingMergeField.SourceField.Split(".").ToList());

                        objectToBeUpdated = JObjectHelper.UpdateObject(objectToBeUpdated, fieldsArr, value);
                        hasChange = true;
                    }
                }
                else if (mappingMergeField.MappingType == MappingType.UpdateValue)
                {
                    var newValue = MapFieldTypes.GetType(mappingMergeField);

                    var fieldsFromDestinationArr = mappingMergeField.DestinationField.Split(".").ToList();
                    //dataQueryMappingType == DataQueryMappingType.UpdateAnotherCollection
                    //? mappingMergeField.DestinationField.Split(".").ToList()
                    //: mappingMergeField.SourceField.Split(".").ToList();

                    objectToBeUpdated = JObjectHelper.UpdateObject(objectToBeUpdated, fieldsFromDestinationArr, newValue);
                    hasChange = true;
                }
                else
                {
                    var valueFromSource =
                        JObjectHelper.GetValueFromObject(sourceObj, mappingMergeField.SourceField.Split(".").ToList());

                    var fieldsFromDestinationArr = mappingMergeField.DestinationField.Split(".").ToList();
                    //dataQueryMappingType == DataQueryMappingType.UpdateAnotherCollection
                    //? mappingMergeField.DestinationField.Split(".").ToList()
                    //    : mappingMergeField.SourceField.Split(".").ToList();

                    objectToBeUpdated = JObjectHelper.UpdateObject(objectToBeUpdated, fieldsFromDestinationArr, valueFromSource);
                    hasChange = true;
                }
            }

            return objectToBeUpdated;
        }
    }
}
