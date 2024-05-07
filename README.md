# Agregátor Objednávek

Navrhněte webovou službu, která:

- Je napsaná v aktuální verzi .NET.
- Nabízí RESP API endpoint pro přijetí jedné nebo více objednávek ve formátu:

  ```json
  [
    {
      "productId": "456",
      "quantity": 5
    },
    {
      "productId": "789",
      "quantity": 42
    }
  ]
  ```

- Objednávky se pro další zpracování agregují - sčítají se počty kusů dle Id produktu.
- Agregované objednávky se ne častěji, než jednou za 20 vteřin, odešlou internímu systému - pro naše účely lze pouze naznačit a vypisovat JSON do konzole.
- Služba by měla počítat s možností velkého množství malých objednávek (stovky za vteřinu) pro relativně limitovaný počet Id produktů (stovky celkem).
- Způsob persistence dat by měl být rozšiřitelný a konfigurovatelný - pro naše účely bude stačit implementovat držení dat v paměti.
- Kód by měl obsahovat (alespoň nějaké) testy.
- Zkuste navrhnout další možná vylepšení a přímo je implementujte nebo jen naznačte / popište.
- Mějte kód takový, jako si představuje v produkční aplikaci.
- Kód odevzdejte nejlépe formou publikace na GitHub - možno i jako privátní repozitář.

---

## Random Considerations throughout Work

- How many requests will go into one output batch?
	- ~ `1000 requests/sec * 20 sec = 20000`
- What is the size of the result JSON?
	- ~ `50 bytes of json * 1000 product IDs = 50kB`
	- seems ok to have in memory and transfer without any splitting or such
- Is it okay to keep product IDs in persistence with a zero quantity?
	- again, this should just be some kB or 10s of kB of "unneeded memory"
	- the reporting job will simply ignore quantities of 0
- Multiple instances of the entire service should work fine behind a load balancer
	- Data will get sent to the core system regardless of which aggregator instance the balancer chose
	- It may be important to consider how often the core system is able to receive from multiple aggregator instances
	- We could just add a final aggregator to collect from all our load-balanced aggregators
- If some asynchronous core system sending was required, it may be a bit of a rework
	- The background worker gets a DI'd object for sending, but the interface for senders is synchronous - the background worker waits until rejection or confirmation by sender and then restarts its timer

## Questions for Team and/or Stakeholders

- Q: Is it necessary to output aggregations of perfectly full orders?
	- Example:
		- Receive order for 1x product A and 1x product B
		- Receive another order for 1x A and 1x B
		- In the middle of processing the second order, the reporting job triggers, and the output is 2x A and 1x B
		- The next output would be 1x B
- Q: Are you sure that this data format is sufficient for your business use case?
	- I would expect more fields to "an order", e.g. customer ID or something
	- There would be a lot of work to redefine "aggregation" if the data was more complex
	- This question is somewhat related to the previous one about keeping track of full orders. The data structures "sufficient" to fulfil the current task are very basic and really just keep numerical sums of each product ID
- Q: Is the product quantity always a whole number?
	- If not, more care will be required to avoid arithmetic errors
	- E.g. specify limited precision of all numbers, or allow some margin of error per order
- Q: What's worse, delivering duplicate quantities, or failing to deliver some?
	- E.g. what about the app crashing at the exact moment after a confirmed delivery to core but before deletion from aggregator persistence?
		- Suggestion: add some kind of sequential ID to requests to core. That way it may be easier to fix live data after the kind of error where core assumes "confirmed" but aggregator assumes "unconfirmed"
		- We can rewrite the aggregator to "prefer" losing orders rather than sending duplicates
- Q: Are there any requirements on authorization, authentication, etc.?

## Todo

- Figure out how to run functional tests against the entire webservice if possible
- Background worker is sending empty orders, make it do nothing and wait for another time period instead
