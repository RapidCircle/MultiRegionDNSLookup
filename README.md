# MultiRegionDNSLookup
Lookup DNS from multiple regions

DNS propogation ideally takes about 3 days but can happen quicker in some regions leading one to falsely believe its done while visitors from the non-propagated regions are left high and dry. 

This tool uses the [DNS Looking Glass](http://www.dns-lg.com/) site to scrape the available country nodes from which DNS lookup can be performed and the tries the lookup from each of them.
Inspired by similar code in ruby by @leighmcculloch
