
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ballons : MonoBehaviour
{

    [SerializeField] Animator[] balloonsAnim;



    void Start()
    {
        StartCoroutine(StartBallonsAnimations());
    }

    IEnumerator StartBallonsAnimations()
    {
        float randomNumber = Random.Range(.5f, 1.2f);

        yield return new WaitForSeconds(randomNumber);
        balloonsAnim[0].enabled = true;

        float randomNumber1 = Random.Range(.8f, 1.5f);

        yield return new WaitForSeconds(randomNumber1);
        balloonsAnim[1].enabled = true;

        float randomNumber2 = Random.Range(1f, 1.4f);

        yield return new WaitForSeconds(randomNumber2);
        balloonsAnim[2].enabled = true;
        float randomNumber3 = Random.Range(.2f, 1f);

        yield return new WaitForSeconds(randomNumber3);
        balloonsAnim[3].enabled = true;
    }


}
