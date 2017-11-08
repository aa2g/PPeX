using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZstdNet
{
    public enum ZSTD_cParameter : Int32
    {
        /* compression format */
        ZSTD_p_format = 10,      /* See ZSTD_format_e enum definition.
                              * Cast selected format as unsigned for ZSTD_CCtx_setParameter() compatibility. */

        /* compression parameters */
        ZSTD_p_compressionLevel = 100, /* Update all compression parameters according to pre-defined cLevel table
                              * Default level is ZSTD_CLEVEL_DEFAULT==3.
                              * Special: value 0 means "do not change cLevel". */
        ZSTD_p_windowLog,        /* Maximum allowed back-reference distance, expressed as power of 2.
                              * Must be clamped between ZSTD_WINDOWLOG_MIN and ZSTD_WINDOWLOG_MAX.
                              * Special: value 0 means "do not change windowLog".
                              * Note: Using a window size greater than ZSTD_MAXWINDOWSIZE_DEFAULT (default: 2^27)
                              * requires setting the maximum window size at least as large during decompression. */
        ZSTD_p_hashLog,          /* Size of the probe table, as a power of 2.
                              * Resulting table size is (1 << (hashLog+2)).
                              * Must be clamped between ZSTD_HASHLOG_MIN and ZSTD_HASHLOG_MAX.
                              * Larger tables improve compression ratio of strategies <= dFast,
                              * and improve speed of strategies > dFast.
                              * Special: value 0 means "do not change hashLog". */
        ZSTD_p_chainLog,         /* Size of the full-search table, as a power of 2.
                              * Resulting table size is (1 << (chainLog+2)).
                              * Larger tables result in better and slower compression.
                              * This parameter is useless when using "fast" strategy.
                              * Special: value 0 means "do not change chainLog". */
        ZSTD_p_searchLog,        /* Number of search attempts, as a power of 2.
                              * More attempts result in better and slower compression.
                              * This parameter is useless when using "fast" and "dFast" strategies.
                              * Special: value 0 means "do not change searchLog". */
        ZSTD_p_minMatch,         /* Minimum size of searched matches (note : repCode matches can be smaller).
                              * Larger values make faster compression and decompression, but decrease ratio.
                              * Must be clamped between ZSTD_SEARCHLENGTH_MIN and ZSTD_SEARCHLENGTH_MAX.
                              * Note that currently, for all strategies < btopt, effective minimum is 4.
                              * Note that currently, for all strategies > fast, effective maximum is 6.
                              * Special: value 0 means "do not change minMatchLength". */
        ZSTD_p_targetLength,     /* Only useful for strategies >= btopt.
                              * Length of Match considered "good enough" to stop search.
                              * Larger values make compression stronger and slower.
                              * Special: value 0 means "do not change targetLength". */
        ZSTD_p_compressionStrategy, /* See ZSTD_strategy enum definition.
                              * Cast selected strategy as unsigned for ZSTD_CCtx_setParameter() compatibility.
                              * The higher the value of selected strategy, the more complex it is,
                              * resulting in stronger and slower compression.
                              * Special: value 0 means "do not change strategy". */

        /* frame parameters */
        ZSTD_p_contentSizeFlag = 200, /* Content size is written into frame header _whenever known_ (default:1)
                              * note that content size must be known at the beginning,
                              * it is sent using ZSTD_CCtx_setPledgedSrcSize() */
        ZSTD_p_checksumFlag,     /* A 32-bits checksum of content is written at end of frame (default:0) */
        ZSTD_p_dictIDFlag,       /* When applicable, dictID of dictionary is provided in frame header (default:1) */

        /* multi-threading parameters */
        ZSTD_p_nbThreads = 400,    /* Select how many threads a compression job can spawn (default:1)
                              * More threads improve speed, but also increase memory usage.
                              * Can only receive a value > 1 if ZSTD_MULTITHREAD is enabled.
                              * Special: value 0 means "do not change nbThreads" */
        ZSTD_p_jobSize,          /* Size of a compression job. Each compression job is completed in parallel.
                              * 0 means default, which is dynamically determined based on compression parameters.
                              * Job size must be a minimum of overlapSize, or 1 KB, whichever is largest
                              * The minimum size is automatically and transparently enforced */
        ZSTD_p_overlapSizeLog,   /* Size of previous input reloaded at the beginning of each job.
                              * 0 => no overlap, 6(default) => use 1/8th of windowSize, >=9 => use full windowSize */

        /* advanced parameters - may not remain available after API update */
        ZSTD_p_forceMaxWindow = 1100, /* Force back-reference distances to remain < windowSize,
                              * even when referencing into Dictionary content (default:0) */
        ZSTD_p_enableLongDistanceMatching = 1200,  /* Enable long distance matching.
                                         * This parameter is designed to improve the compression
                                         * ratio for large inputs with long distance matches.
                                         * This increases the memory usage as well as window size.
                                         * Note: setting this parameter sets all the LDM parameters
                                         * as well as ZSTD_p_windowLog. It should be set after
                                         * ZSTD_p_compressionLevel and before ZSTD_p_windowLog and
                                         * other LDM parameters. Setting the compression level
                                         * after this parameter overrides the window log, though LDM
                                         * will remain enabled until explicitly disabled. */
        ZSTD_p_ldmHashLog,   /* Size of the table for long distance matching, as a power of 2.
                          * Larger values increase memory usage and compression ratio, but decrease
                          * compression speed.
                          * Must be clamped between ZSTD_HASHLOG_MIN and ZSTD_HASHLOG_MAX
                          * (default: windowlog - 7). */
        ZSTD_p_ldmMinMatch,  /* Minimum size of searched matches for long distance matcher.
                          * Larger/too small values usually decrease compression ratio.
                          * Must be clamped between ZSTD_LDM_MINMATCH_MIN
                          * and ZSTD_LDM_MINMATCH_MAX (default: 64). */
        ZSTD_p_ldmBucketSizeLog,  /* Log size of each bucket in the LDM hash table for collision resolution.
                               * Larger values usually improve collision resolution but may decrease
                               * compression speed.
                               * The maximum value is ZSTD_LDM_BUCKETSIZELOG_MAX (default: 3). */
        ZSTD_p_ldmHashEveryLog,  /* Frequency of inserting/looking up entries in the LDM hash table.
                              * The default is MAX(0, (windowLog - ldmHashLog)) to
                              * optimize hash table usage.
                              * Larger values improve compression speed. Deviating far from the
                              * default value will likely result in a decrease in compression ratio.
                              * Must be clamped between 0 and ZSTD_WINDOWLOG_MAX - ZSTD_HASHLOG_MIN. */
    }
}
