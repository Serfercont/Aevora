using System.Text;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class FinalCinematicManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI cinematicTextDisplay;

    private DialogueRunner dialogueRunner;

    void Start()
    {
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        
        if (cinematicTextDisplay == null)
        {
            return;
        }

        BuildAndShowCinematic();
    }

    private void BuildAndShowCinematic()
    {
        StringBuilder finalStory = new StringBuilder();
        
        finalStory.AppendLine("Con el tiempo en mi contra y las sombras del Ojo Imperial acechando, logré escapar de las entrañas de la base Vickers. Los documentos que prueban su negligencia en el incidente Overlord están por fin en nuestras manos. Nuestra misión fue un éxito. Ante la humillación, el Imperio necesitaba respuestas; Bartholomew Graves, el comandante que no supo ver bajo sus propias narices, fue considerado responsable del fallo de seguridad, siendo despojado de su cargo y castigado. La maquinaria del Imperio sangra, y nosotros hemos dado el primer corte.\n");

        var variableStorage = dialogueRunner != null ? dialogueRunner.VariableStorage : null;

        bool ayudoAZeke = false;
        bool vistoRichieTommy = false;
        bool ayudoATommy = false;

        if (variableStorage != null)
        {
            variableStorage.TryGetValue("$ayudoAZeke", out ayudoAZeke);
            variableStorage.TryGetValue("$vistoRichieTommy", out vistoRichieTommy);
            variableStorage.TryGetValue("$ayudoATommy", out ayudoATommy);
        }

        if (ayudoAZeke)
        {
            finalStory.AppendLine("Arriba, Zeke Higgs logró salir de aquel infierno con vida gracias a los suministros médicos que le dejé. Respiró el aire libre por un breve instante. Sin embargo, en el Imperio de Aevora la libertad es efímera; fue capturado poco después y ejecutado sin piedad. Al menos, murió sabiendo que no se rindió en esa habitación.\n");
        }
        else
        {
            finalStory.AppendLine("En el primer piso, el cuerpo destrozado de Zeke exhaló su último aliento. Sin ayuda médica, las brutales heridas de la tortura imperial terminaron consumiéndolo antes de que pudiera volver a ver la luz del día. Murió en la oscuridad, en total soledad.\n");
        }

        if (vistoRichieTommy) 
        {
            if (ayudoATommy)
            {
                finalStory.AppendLine("A pesar del infierno, no dejé a los míos atrás. Con las heridas vendadas, Thomas Tanner volvió a alzarse como el líder que necesitábamos. Aprovechando el caos que dejamos a nuestro paso, guio a los prisioneros hacia la salida, logrando escapar de las garras de los Vickers.\n");
            }
            else
            {
                finalStory.AppendLine("Abrí las celdas, pero Thomas ya no tenía fuerzas para guiarlos; su vida se apagó en aquel sótano. Sin su líder, la huida de mis compañeros fue desesperada y desorganizada. Intentaron escapar por su cuenta entre los pasillos, pero la mayoría fueron cazados y ejecutados por las patrullas imperiales.\n");
            }
        }
        else
        {
            finalStory.AppendLine("Elegí la misión por encima de mis hermanos. Las celdas permanecieron cerradas, abandonándolos a su suerte. Seguirán en aquella oscuridad, sometidos a los brutales interrogatorios y a la tortura sin fin del Ojo Imperial.\n");
        }

        finalStory.AppendLine("La base Vickers permaneció intacta, como un frío monumento al control imperial. No hubo explosiones que enmascararan el suceso, pero la humillación fue suficiente. Todos los guardias implicados en aquel turno sufrieron severas sanciones por haber permitido que nuestra sombra cruzara sus pasillos.\n");

        if (SpatialNarrativeInteractable.FoundZekeReport)
        {
            finalStory.AppendLine("Y lo más importante: recuperé el informe de los interrogatorios. Los secretos que le arrancaron a Zeke bajo el dolor de los vapores cáusticos no llegarán al Alto Mando. Las ubicaciones de nuestros pisos francos y los nombres de nuestros infiltrados están a salvo. Hoy, el Imperio ha perdido, y nosotros tenemos una oportunidad para seguir en el mañana.");
        }
        else
        {
            finalStory.AppendLine("Sin embargo, dejamos atrás un cabo suelto mortal. El informe con las confesiones que le arrancaron a Zeke llegó a manos de la Red del Ojo Imperial. En los días que siguieron, varias bases de nuestra resistencia fueron localizadas y destruidas. El movimiento ha sufrido un golpe devastador del que tardaremos años en recuperarnos... si es que logramos sobrevivir.");
        }

        cinematicTextDisplay.text = finalStory.ToString();
    }
}