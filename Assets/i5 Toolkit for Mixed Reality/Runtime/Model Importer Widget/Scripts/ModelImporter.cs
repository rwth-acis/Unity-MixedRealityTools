using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.Utilities.UnityWrappers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporter
    {
        public IModelProvider CurrentlySelectedProvider { get; set; }

        public ITransformable TargetTransform { get; set; }

        public Bounds TargetBox { get; set; }

        public List<IModelImportPostProcessor> PostProcessors { get; set; }

        public ModelImporter(ITransformable targetTransform, Bounds targetBox)
        {
            TargetTransform = targetTransform;
            TargetBox = targetBox;
            PostProcessors = new List<IModelImportPostProcessor>()
            {
                new MovablePostProcessor()
            };
        }

        public async Task ImportModelAsync(string modelId)
        {
            GameObject importedModel = await CurrentlySelectedProvider.ProvideModelAsync(modelId);

            Bounds overallBounds = ObjectBounds.GetComposedRendererBounds(importedModel);

            importedModel.transform.position = 
                TargetTransform.Position
                + TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.center)
                - overallBounds.center;
            importedModel.transform.rotation = TargetTransform.Rotation;

            Vector3 realTargetBoxSize = TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.size);

            Vector3 scalingFactors = realTargetBoxSize.DivideComponentWiseBy(overallBounds.size);
            float scalingFactor = scalingFactors.MinimumComponent();
            importedModel.transform.localScale *= scalingFactor;

            foreach(IModelImportPostProcessor postProcessor in PostProcessors)
            {
                postProcessor.PostProcessGameObject(importedModel);
            }
        }
    }
}